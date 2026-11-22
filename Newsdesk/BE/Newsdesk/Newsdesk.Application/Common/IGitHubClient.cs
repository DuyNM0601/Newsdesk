namespace Newsdesk.Application.Common
{
    /// <summary>Thông tin tối thiểu của một repo, đủ để hiển thị và đưa cho AI. Repo fork / đã lưu trữ đã bị loại ở tầng cài đặt.</summary>
    public sealed record GitHubRepoInfo(
        string FullName,
        string Url,
        string? Description,
        string? Language,
        string? License,
        IReadOnlyList<string> Topics,
        int Stars,
        DateTimeOffset CreatedAt);

    /// <summary>GitHub không dùng được (hết hạn mức, mất mạng, token sai). Người gọi dừng và báo lỗi, không đoán.</summary>
    public sealed class GitHubUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

    /// <summary>
    /// Gọi GitHub API công khai (miễn phí) với số lần gọi ít nhất: MỘT lần tìm kiếm cho cả ngày, rồi README của vài repo được chọn.
    /// Application chỉ biết interface này; Infrastructure cài đặt bằng HttpClient.
    /// </summary>
    public interface IGitHubClient
    {
        /// <summary>Repo tạo sau createdAfter, ít nhất minStars sao, nhiều sao nhất trước (tối đa 100). Đúng MỘT request.</summary>
        Task<IReadOnlyList<GitHubRepoInfo>> SearchRisingAsync(DateTimeOffset createdAfter, int minStars, CancellationToken cancellationToken);

        /// <summary>README dạng markdown thô; null nếu repo không có README.</summary>
        Task<string?> GetReadmeAsync(string fullName, CancellationToken cancellationToken);
    }
}
