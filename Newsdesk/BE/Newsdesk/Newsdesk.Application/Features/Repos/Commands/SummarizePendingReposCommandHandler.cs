using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.News;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Repos.Commands
{
    public sealed class SummarizePendingReposCommandHandler(
        IAppDbContext db,
        IArticleSummarizer summarizer,
        TimeProvider clock,
        ILogger<SummarizePendingReposCommandHandler> logger)
        : CommandHandler<SummarizePendingReposCommand, int>(logger)
    {
        public override async Task<Result<int>> HandleAsync(SummarizePendingReposCommand command, CancellationToken ct)
        {
            long start = clock.GetTimestamp();

            List<Repo> pending = await db.Repos
                .Where(r => r.SummaryStatus == null || (r.SummaryStatus == SummaryStatus.Failed && r.Attempts < RepoRules.MaxAttempts))
                .OrderByDescending(r => r.DiscoveredOn).ThenBy(r => r.Rank)
                .Take(command.MaxCount)
                .ToListAsync(ct);

            int succeeded = 0;
            foreach (Repo repo in pending)
            {
                if (clock.GetElapsedTime(start) > command.Budget)
                    break;

                repo.Attempts++;
                try
                {
                    string excerpt = $"{repo.Description}\n\nTopics: {repo.Topics}\nLanguage: {repo.Language}\n\n{repo.ReadmeExcerpt}";
                    SummaryResult r = await summarizer.SummarizeAsync(
                        new SummarizeInput(repo.FullName, excerpt, "GitHub", repo.Language ?? "", SummarizeKind.Repo), ct);
                    repo.SummaryStatus = SummaryStatus.Success;
                    repo.Tldr = NewsRules.Truncate(r.Tldr, 600);
                    repo.KeyPoints = JsonSerializer.Serialize(r.KeyPoints.Select(p => NewsRules.Truncate(p.Trim(), 300)).Take(5));
                    repo.WhyItMatters = NewsRules.Truncate(r.WhyItMatters, 400);
                    repo.SummaryLanguage = r.Language;
                    repo.Model = NewsRules.Truncate(r.Model, 100);
                    repo.PromptVersion = NewsRules.Truncate(r.PromptVersion, 20);
                    repo.Error = null;
                    repo.ReadmeExcerpt = null; // đã tóm tắt xong, không giữ lại văn bản README
                    succeeded++;
                }
                catch (SummarizerUnavailableException ex)
                {
                    // AI chưa chạy: dừng cả lô, không tính lần thử.
                    logger.LogWarning("AI summarizer unavailable, stopping repo batch: {Message}", ex.Message);
                    repo.Attempts--;
                    await db.SaveChangesAsync(ct);
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Summarizing repo {Repo} failed (attempt {Attempt}).", repo.FullName, repo.Attempts);
                    repo.SummaryStatus = SummaryStatus.Failed;
                    repo.Error = NewsRules.Truncate(ex.Message, 500);
                }

                await db.SaveChangesAsync(ct);
            }

            return Ok(succeeded);
        }
    }
}
