using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newsdesk.Application.Common;

namespace Newsdesk.Infrastructure.News
{
    /// <summary>
    /// Đọc feed RSS 2.0 và Atom bằng System.Xml.Linq (không thêm package). Chỉ lấy tiêu đề, link, đoạn trích (đã bỏ HTML),
    /// ảnh và ngày đăng. Mặc định XmlReader cấm DTD nên không dính XXE.
    /// </summary>
    public sealed partial class RssFeedFetcher(HttpClient http) : IFeedFetcher
    {
        public async Task<IReadOnlyList<FeedItem>> FetchAsync(string feedUrl, CancellationToken cancellationToken)
        {
            await using Stream stream = await http.GetStreamAsync(feedUrl, cancellationToken);
            XDocument doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

            // RSS: <item>; Atom: <entry>. So khớp theo tên local để khỏi phụ thuộc namespace.
            IEnumerable<XElement> entries = doc.Descendants().Where(e => e.Name.LocalName is "item" or "entry");

            var items = new List<FeedItem>();
            foreach (XElement e in entries)
            {
                string? title = Clean(Child(e, "title")?.Value);
                string? link = LinkOf(e);
                if (title is null || link is null)
                    continue;

                string? summary = Clean(Child(e, "description")?.Value ?? Child(e, "summary")?.Value ?? Child(e, "encoded")?.Value ?? Child(e, "content")?.Value);
                string? date = Child(e, "pubDate")?.Value ?? Child(e, "published")?.Value ?? Child(e, "updated")?.Value ?? Child(e, "date")?.Value;
                DateTimeOffset? published = DateTimeOffset.TryParse(date, out DateTimeOffset p) ? p.ToUniversalTime() : null;

                items.Add(new FeedItem(title, link, summary is { Length: > 1000 } ? summary[..1000] : summary, ImageOf(e), published));
            }

            return items;
        }

        private static XElement? Child(XElement parent, string localName) =>
            parent.Elements().FirstOrDefault(c => c.Name.LocalName == localName);

        // RSS: <link>url</link>. Atom: <link rel="alternate" href="url"/> (rel mặc định là alternate).
        private static string? LinkOf(XElement e)
        {
            foreach (XElement link in e.Elements().Where(c => c.Name.LocalName == "link"))
            {
                string? rel = (string?)link.Attribute("rel");
                if (rel is not null && rel != "alternate")
                    continue;
                string? url = (string?)link.Attribute("href") ?? link.Value;
                if (!string.IsNullOrWhiteSpace(url))
                    return url.Trim();
            }
            return null;
        }

        // Ảnh: media:content / media:thumbnail (url=...) hoặc <enclosure type="image/..." url=...>.
        private static string? ImageOf(XElement e)
        {
            foreach (XElement c in e.Elements())
            {
                string name = c.Name.LocalName;
                string? url = (string?)c.Attribute("url");
                if (url is null)
                    continue;
                if (name == "thumbnail")
                    return url;
                if (name is "content" or "enclosure")
                {
                    string? type = (string?)c.Attribute("type") ?? (string?)c.Attribute("medium");
                    if (type is null || type.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                        return url;
                }
            }
            return null;
        }

        // Bỏ thẻ HTML, giải mã &amp; ..., gộp khoảng trắng. Trả null nếu rỗng.
        private static string? Clean(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;
            string text = WebUtility.HtmlDecode(TagRegex().Replace(html, " "));
            text = SpaceRegex().Replace(text, " ").Trim();
            return text.Length == 0 ? null : text;
        }

        [GeneratedRegex("<[^>]+>")]
        private static partial Regex TagRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex SpaceRegex();
    }
}
