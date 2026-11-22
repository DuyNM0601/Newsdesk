using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.Digest;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Repos.Commands
{
    public sealed class FetchTrendingReposCommandHandler(IAppDbContext db, IGitHubClient github, TimeProvider clock, ILogger<FetchTrendingReposCommandHandler> logger)
        : CommandHandler<FetchTrendingReposCommand, int>(logger)
    {
        public override async Task<Result<int>> HandleAsync(FetchTrendingReposCommand command, CancellationToken ct)
        {
            DateTimeOffset now = clock.GetUtcNow();
            DateOnly today = DigestRules.TodayInVietnam(now);
            if (await db.Repos.AnyAsync(r => r.DiscoveredOn == today, ct))
                return Ok(0);

            IReadOnlyList<GitHubRepoInfo> found;
            try
            {
                found = await github.SearchRisingAsync(now.AddDays(-RepoRules.WindowDays), RepoRules.MinStars, ct);
            }
            catch (GitHubUnavailableException ex)
            {
                return Fail(Error.Conflict(ex.Message));
            }

            // Bỏ repo đã giới thiệu và danh sách "awesome-...", AI trước rồi tới nhiều sao.
            string[] names = found.Select(f => f.FullName).ToArray();
            HashSet<string> already = (await db.Repos.Where(r => names.Contains(r.FullName)).Select(r => r.FullName).ToListAsync(ct)).ToHashSet();
            var candidates = found
                .Where(f => !already.Contains(f.FullName) && !RepoRules.IsLinkList(f.FullName, f.Topics))
                .Select(f => (Info: f, Kind: RepoRules.KindOf(f.FullName, f.Description, f.Topics)))
                .OrderBy(c => c.Kind == RepoKind.Ai ? 0 : 1)
                .ThenByDescending(c => c.Info.Stars)
                .ToList();

            int rank = 0;
            foreach (var (info, kind) in candidates)
            {
                if (rank >= RepoRules.DailyCount)
                    break;

                string? readme;
                try
                {
                    readme = await github.GetReadmeAsync(info.FullName, ct);
                }
                catch (GitHubUnavailableException ex)
                {
                    logger.LogWarning("GitHub stopped answering while fetching READMEs: {Message}", ex.Message);
                    break;
                }

                // Repo không có README đủ nội dung thì không có gì để tóm tắt trung thực: bỏ và lấy repo kế tiếp.
                string excerpt = RepoRules.CleanReadme(readme ?? "");
                if (excerpt.Length < RepoRules.MinReadmeLength)
                    continue;

                db.Repos.Add(new Repo
                {
                    FullName = info.FullName,
                    Url = info.Url,
                    Description = info.Description is null ? null : Truncate(info.Description, 500),
                    Language = info.Language,
                    License = info.License,
                    Topics = Truncate(string.Join(',', info.Topics), 300),
                    Stars = info.Stars,
                    Kind = kind,
                    CreatedOnGitHub = info.CreatedAt,
                    DiscoveredOn = today,
                    Rank = ++rank,
                    ReadmeExcerpt = excerpt,
                    CreatedAt = now,
                });
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Picked {Count} trending repo(s) for {Date}.", rank, today);
            return Ok(rank);
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
    }
}
