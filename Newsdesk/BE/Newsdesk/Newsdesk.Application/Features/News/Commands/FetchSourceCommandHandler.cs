using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.News.Commands
{
    public sealed class FetchSourceCommandHandler(
        IAppDbContext db,
        IFeedFetcher fetcher,
        TimeProvider clock,
        ILogger<FetchSourceCommandHandler> logger)
        : CommandHandler<FetchSourceCommand, int>(logger)
    {
        public override async Task<Result<int>> HandleAsync(FetchSourceCommand command, CancellationToken ct)
        {
            Source? source = await db.Sources.FirstOrDefaultAsync(x => x.Id == command.SourceId, ct);
            if (source is null)
                return Fail(Error.NotFound("Source not found."));
            if (source.Status != SourceStatus.Active)
                return Ok(0);

            DateTimeOffset now = clock.GetUtcNow();
            IReadOnlyList<FeedItem> items;
            try
            {
                items = await fetcher.FetchAsync(source.FeedUrl, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Nguồn lỗi là chuyện thường (mạng, feed đổi địa chỉ): ghi lại để người quản trị thấy, không làm hỏng cả nhịp.
                logger.LogWarning(ex, "Fetching feed of source {Source} failed.", source.Name);
                source.LastFetchedAt = now;
                source.LastFetchStatus = FetchStatus.Failed;
                source.LastError = NewsRules.Truncate(ex.Message, 500);
                await db.SaveChangesAsync(ct);
                return Ok(0);
            }

            // Chuẩn hoá link, bỏ mục không hợp lệ và trùng link trong cùng một lần tải.
            var candidates = new Dictionary<string, (FeedItem Item, string Url)>();
            foreach (FeedItem item in items.Take(NewsRules.MaxItemsPerFetch))
            {
                string? url = NewsRules.NormalizeUrl(item.Url);
                if (url is null || string.IsNullOrWhiteSpace(item.Title))
                    continue;
                candidates.TryAdd(NewsRules.HashUrl(url), (item, url));
            }

            List<string> hashes = candidates.Keys.ToList();
            HashSet<string> existing = (await db.Articles
                .Where(x => hashes.Contains(x.UrlHash))
                .Select(x => x.UrlHash)
                .ToListAsync(ct)).ToHashSet();

            int added = 0;
            foreach (var (hash, (item, url)) in candidates)
            {
                if (existing.Contains(hash))
                    continue;

                // Ngày trong tương lai (feed sai múi giờ) bị kéo về hiện tại để không đứng đầu danh sách mãi.
                DateTimeOffset published = item.PublishedAt is { } p && p <= now ? p : now;
                (int priority, string topics) = NewsRules.Classify(source.Category, item.Title, item.Summary);
                db.Articles.Add(new Article
                {
                    SourceId = source.Id,
                    Title = NewsRules.Truncate(item.Title.Trim(), 500),
                    Url = NewsRules.Truncate(url, 2048),
                    UrlHash = hash,
                    Summary = item.Summary is null ? null : NewsRules.Truncate(item.Summary, 1000),
                    ImageUrl = NewsRules.NormalizeUrl(item.ImageUrl) is { } img ? NewsRules.Truncate(img, 2048) : null,
                    Category = source.Category,
                    PublishedAt = published,
                    FetchedAt = now,
                    Priority = priority,
                    Topics = topics,
                });
                added++;
            }

            source.LastFetchedAt = now;
            source.LastFetchStatus = FetchStatus.Success;
            source.LastError = null;

            // shortcut: hai lần tải song song cùng một nguồn sẽ đụng unique UrlHash và lần sau thất bại; hiện chỉ một job tuần tự
            // gọi command này. Khi tách sang RabbitMQ (at-least-once) phải bắt DbUpdateException và coi là "đã có".
            await db.SaveChangesAsync(ct);
            return Ok(added);
        }
    }
}
