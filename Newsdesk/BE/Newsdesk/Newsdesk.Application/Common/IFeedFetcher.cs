namespace Newsdesk.Application.Common
{
    /// <summary>Một mục trong feed, đã bỏ HTML khỏi Summary. PublishedAt null nếu feed không có ngày.</summary>
    public sealed record FeedItem(string Title, string Url, string? Summary, string? ImageUrl, DateTimeOffset? PublishedAt);

    /// <summary>Tải và đọc một feed RSS/Atom. Infrastructure cài đặt (HTTP + XML), Application không biết mạng.</summary>
    public interface IFeedFetcher
    {
        /// <summary>Ném exception nếu không tải/đọc được feed; người gọi ghi lỗi vào nguồn tin.</summary>
        Task<IReadOnlyList<FeedItem>> FetchAsync(string feedUrl, CancellationToken cancellationToken);
    }
}
