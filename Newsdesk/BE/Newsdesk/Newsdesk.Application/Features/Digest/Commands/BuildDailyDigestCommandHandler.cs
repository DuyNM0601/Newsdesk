using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Digest.Commands
{
    public sealed class BuildDailyDigestCommandHandler(IAppDbContext db, TimeProvider clock, ILogger<BuildDailyDigestCommandHandler> logger)
        : CommandHandler<BuildDailyDigestCommand, int>(logger)
    {
        private sealed record Candidate(string Id, string SourceId, string Category);

        public override async Task<Result<int>> HandleAsync(BuildDailyDigestCommand command, CancellationToken ct)
        {
            DateTimeOffset now = clock.GetUtcNow();
            DateOnly date = DigestRules.TodayInVietnam(now);

            bool alreadyBuilt = command.OnlyIfNoDigestYet
                ? await db.DigestEntries.AnyAsync(ct)
                : await db.DigestEntries.AnyAsync(x => x.DigestDate == date, ct);
            if (alreadyBuilt)
                return Ok(0);

            DateTimeOffset since = now - DigestRules.Window;

            // Ứng viên: bài mới, có đoạn trích đủ dài (để tóm tắt trung thực), chưa nằm trong bản tin nào, điểm cao trước.
            List<Candidate> candidates = await db.Articles
                .AsNoTracking()
                .Where(a => a.PublishedAt >= since
                    && a.Summary != null && a.Summary.Length >= DigestRules.MinExcerptLength
                    && !db.DigestEntries.Any(e => e.ArticleId == a.Id))
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.PublishedAt)
                .Take(400)
                .Select(a => new Candidate(a.Id, a.SourceId, a.Category))
                .ToListAsync(ct);

            HashSet<string> chosen = SelectArticles(candidates);
            if (chosen.Count == 0)
                return Ok(0);

            // Giữ thứ tự điểm giảm dần của danh sách ứng viên làm thứ hạng trong ngày.
            int rank = 0;
            foreach (Candidate c in candidates.Where(c => chosen.Contains(c.Id)))
            {
                db.DigestEntries.Add(new DigestEntry { DigestDate = date, Rank = ++rank, ArticleId = c.Id, CreatedAt = now });
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Built digest for {Date} with {Count} article(s).", date, rank);
            return Ok(rank);
        }

        /// <summary>Lượt 1: theo hạn ngạch từng mục và trần mỗi nguồn. Lượt 2: lấp chỗ trống bằng bài điểm cao nhất còn lại (vẫn giữ trần mỗi nguồn).</summary>
        private static HashSet<string> SelectArticles(List<Candidate> candidates)
        {
            var chosen = new HashSet<string>();
            var perSource = new Dictionary<string, int>();
            var perCategory = new Dictionary<string, int>();

            bool Take(Candidate c, bool useQuota)
            {
                if (chosen.Contains(c.Id)) return false;
                if (perSource.GetValueOrDefault(c.SourceId) >= DigestRules.MaxPerSource) return false;
                if (useQuota && perCategory.GetValueOrDefault(c.Category) >= DigestRules.CategoryQuota.GetValueOrDefault(c.Category, 0)) return false;
                chosen.Add(c.Id);
                perSource[c.SourceId] = perSource.GetValueOrDefault(c.SourceId) + 1;
                perCategory[c.Category] = perCategory.GetValueOrDefault(c.Category) + 1;
                return true;
            }

            foreach (Candidate c in candidates)
            {
                if (chosen.Count >= DigestRules.Size) break;
                Take(c, useQuota: true);
            }
            foreach (Candidate c in candidates)
            {
                if (chosen.Count >= DigestRules.Size) break;
                Take(c, useQuota: false);
            }
            return chosen;
        }
    }
}
