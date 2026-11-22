namespace Newsdesk.Application.Common
{
    /// <summary>Loại nội dung cần tóm tắt: quyết định cách viết prompt.</summary>
    public static class SummarizeKind
    {
        public const string Article = "Article";
        public const string Repo = "Repo";
    }

    /// <summary>
    /// Thứ model cần để tóm tắt. Bài báo: tiêu đề + đoạn trích của feed (không có toàn văn). Repo: tên + mô tả + đầu README.
    /// </summary>
    public sealed record SummarizeInput(string Title, string Excerpt, string SourceName, string Category, string Kind = SummarizeKind.Article);

    public sealed record SummaryResult(string Tldr, IReadOnlyList<string> KeyPoints, string WhyItMatters, string Language, string Model, string PromptVersion);

    /// <summary>Dịch vụ AI chưa sẵn sàng (chưa cài, chưa chạy, hết hạn mức). Người gọi dừng cả lô thay vì tính là bài lỗi.</summary>
    public sealed class SummarizerUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

    /// <summary>
    /// Tóm tắt một bài hoặc một repo bằng model AI. Application chỉ biết interface này; Infrastructure cài đặt (Ollama chạy local, sau này có thể là Claude API).
    /// Ném SummarizerUnavailableException nếu dịch vụ không dùng được, ném exception khác nếu model trả kết quả không hợp lệ.
    /// </summary>
    public interface IArticleSummarizer
    {
        Task<SummaryResult> SummarizeAsync(SummarizeInput input, CancellationToken cancellationToken);
    }
}
