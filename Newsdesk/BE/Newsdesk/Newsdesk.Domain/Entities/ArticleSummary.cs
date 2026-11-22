using System.ComponentModel.DataAnnotations;

namespace Newsdesk.Domain.Entities;

/// <summary>Kết quả tóm tắt của một bài, lưu dạng chuỗi trong cột ArticleSummary.Status.</summary>
public static class SummaryStatus
{
    public const string Success = "Success";
    public const string Failed = "Failed";
}

/// <summary>
/// Bản tóm tắt AI của MỘT bài (tối đa một dòng cho mỗi bài). Tạo nền bằng cron, đọc sẵn ra cho người dùng:
/// không bao giờ gọi AI trong request của người đọc. Nội dung chỉ dựa trên tiêu đề + đoạn trích của feed, không phải toàn văn.
/// </summary>
public class ArticleSummary : BaseEntity
{
    [MaxLength(36)]
    public string ArticleId { get; set; } = default!;

    /// <summary>SummaryStatus.Success / Failed.</summary>
    [MaxLength(20)]
    public string Status { get; set; } = SummaryStatus.Failed;

    /// <summary>Ý chính trong một hai câu.</summary>
    [MaxLength(600)]
    public string Tldr { get; set; } = "";

    /// <summary>Các ý chính, mảng JSON các chuỗi, ví dụ ["...","..."].</summary>
    [MaxLength(2000)]
    public string KeyPoints { get; set; } = "[]";

    /// <summary>Một câu: vì sao đáng quan tâm với người làm phần mềm (rỗng nếu không liên quan).</summary>
    [MaxLength(400)]
    public string WhyItMatters { get; set; } = "";

    /// <summary>Mã ngôn ngữ của bản tóm tắt ("en", "vi").</summary>
    [MaxLength(10)]
    public string Language { get; set; } = "en";

    [MaxLength(100)]
    public string Model { get; set; } = "";

    /// <summary>Đổi khi đổi prompt, để biết bản tóm tắt nào sinh bằng prompt cũ.</summary>
    [MaxLength(20)]
    public string PromptVersion { get; set; } = "";

    /// <summary>Số lần đã thử (cả thất bại); dừng thử lại khi tới DigestRules.MaxAttempts.</summary>
    public int Attempts { get; set; }

    [MaxLength(500)]
    public string? Error { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
