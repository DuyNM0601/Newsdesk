using Microsoft.EntityFrameworkCore;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Infrastructure.Data
{
    /// <summary>
    /// Nạp danh sách nguồn tin ban đầu (RSS chính thống). Chỉ thêm nguồn CHƯA có (theo FeedUrl), không ghi đè dòng đã có,
    /// nên tắt/sửa nguồn trong database không bị mất khi khởi động lại.
    /// </summary>
    public static class SourceSeeder
    {
        private static readonly (string Name, string Url, string Category)[] Defaults =
        [
            ("Ars Technica", "https://feeds.arstechnica.com/arstechnica/index", NewsCategory.Tech),
            ("The Verge", "https://www.theverge.com/rss/index.xml", NewsCategory.Tech),
            ("MIT Technology Review", "https://www.technologyreview.com/feed/", NewsCategory.Tech),
            ("Wired", "https://www.wired.com/feed/rss", NewsCategory.Tech),

            (".NET Blog", "https://devblogs.microsoft.com/dotnet/feed/", NewsCategory.Programming),
            ("GitHub Blog", "https://github.blog/feed/", NewsCategory.Programming),
            ("Stack Overflow Blog", "https://stackoverflow.blog/feed/", NewsCategory.Programming),
            ("InfoQ", "https://feed.infoq.com/", NewsCategory.Programming),
            ("Cloudflare Blog", "https://blog.cloudflare.com/rss/", NewsCategory.Programming),

            ("BBC News - World", "https://feeds.bbci.co.uk/news/world/rss.xml", NewsCategory.Politics),
            ("The Guardian - World", "https://www.theguardian.com/world/rss", NewsCategory.Politics),
            ("Al Jazeera", "https://www.aljazeera.com/xml/rss/all.xml", NewsCategory.Politics),
            ("NPR - World", "https://feeds.npr.org/1004/rss.xml", NewsCategory.Politics),

            ("BBC News - Business", "https://feeds.bbci.co.uk/news/business/rss.xml", NewsCategory.Economy),
            ("The Guardian - Business", "https://www.theguardian.com/uk/business/rss", NewsCategory.Economy),
            ("CNBC", "https://www.cnbc.com/id/100003114/device/rss/rss.html", NewsCategory.Economy),
        ];

        public static async Task SeedAsync(AppDbContext db, TimeProvider clock, CancellationToken ct = default)
        {
            HashSet<string> existing = (await db.Sources.Select(x => x.FeedUrl).ToListAsync(ct)).ToHashSet();
            foreach (var (name, url, category) in Defaults)
            {
                if (existing.Contains(url))
                    continue;
                db.Sources.Add(new Source { Name = name, FeedUrl = url, Category = category, CreatedAt = clock.GetUtcNow() });
            }
            await db.SaveChangesAsync(ct);
        }
    }
}
