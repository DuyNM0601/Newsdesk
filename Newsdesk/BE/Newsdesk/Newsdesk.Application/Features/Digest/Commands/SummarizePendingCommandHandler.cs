using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.News;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Digest.Commands
{
    public sealed class SummarizePendingCommandHandler(
        IAppDbContext db,
        IArticleSummarizer summarizer,
        TimeProvider clock,
        ILogger<SummarizePendingCommandHandler> logger)
        : CommandHandler<SummarizePendingCommand, int>(logger)
    {
        public override async Task<Result<int>> HandleAsync(SummarizePendingCommand command, CancellationToken ct)
        {
            long start = clock.GetTimestamp();

            // Chưa tóm tắt thành công và chưa hết số lần thử. Ngày mới nhất, hạng cao nhất trước.
            var pending = await (
                from e in db.DigestEntries
                join a in db.Articles on e.ArticleId equals a.Id
                where !db.ArticleSummaries.Any(s => s.ArticleId == a.Id && (s.Status == SummaryStatus.Success || s.Attempts >= DigestRules.MaxAttempts))
                orderby e.DigestDate descending, e.Rank
                select new { a.Id, a.Title, a.Summary, SourceName = a.Source.Name, a.Category })
                .AsNoTracking()
                .Take(command.MaxCount)
                .ToListAsync(ct);

            int succeeded = 0;
            foreach (var p in pending)
            {
                if (clock.GetElapsedTime(start) > command.Budget)
                    break;

                ArticleSummary? row = await db.ArticleSummaries.FirstOrDefaultAsync(s => s.ArticleId == p.Id, ct);
                if (row is null)
                {
                    row = new ArticleSummary { ArticleId = p.Id, CreatedAt = clock.GetUtcNow() };
                    db.ArticleSummaries.Add(row);
                }
                row.Attempts++;
                row.UpdatedAt = clock.GetUtcNow();

                try
                {
                    SummaryResult r = await summarizer.SummarizeAsync(new SummarizeInput(p.Title, p.Summary ?? "", p.SourceName, p.Category), ct);
                    row.Status = SummaryStatus.Success;
                    row.Tldr = NewsRules.Truncate(r.Tldr, 600);
                    row.KeyPoints = SerializeKeyPoints(r.KeyPoints);
                    row.WhyItMatters = NewsRules.Truncate(r.WhyItMatters, 400);
                    row.Language = r.Language;
                    row.Model = NewsRules.Truncate(r.Model, 100);
                    row.PromptVersion = NewsRules.Truncate(r.PromptVersion, 20);
                    row.Error = null;
                    succeeded++;
                }
                catch (SummarizerUnavailableException ex)
                {
                    // Dịch vụ AI chưa chạy: dừng cả lô, KHÔNG tính là lần thử của bài (hoàn lại) để khi bật AI lên là tóm tắt được ngay.
                    logger.LogWarning("AI summarizer unavailable, stopping this batch: {Message}", ex.Message);
                    row.Attempts--;
                    if (row.Attempts == 0)
                        db.ArticleSummaries.Remove(row);
                    await db.SaveChangesAsync(ct);
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Summarizing article {ArticleId} failed (attempt {Attempt}).", p.Id, row.Attempts);
                    row.Status = SummaryStatus.Failed;
                    row.Error = NewsRules.Truncate(ex.Message, 500);
                }

                await db.SaveChangesAsync(ct);
            }

            return Ok(succeeded);
        }

        /// <summary>Mảng JSON các ý chính, cắt bớt cho vừa cột (2000 ký tự).</summary>
        private static string SerializeKeyPoints(IReadOnlyList<string> points)
        {
            var kept = new List<string>();
            foreach (string point in points)
            {
                kept.Add(NewsRules.Truncate(point.Trim(), 300));
                if (JsonSerializer.Serialize(kept).Length > 2000)
                {
                    kept.RemoveAt(kept.Count - 1);
                    break;
                }
            }
            return JsonSerializer.Serialize(kept);
        }
    }
}
