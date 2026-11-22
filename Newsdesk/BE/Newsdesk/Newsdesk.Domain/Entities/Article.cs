using System.ComponentModel.DataAnnotations;

namespace Newsdesk.Domain.Entities;

/// <summary>
/// Một bài tin lấy từ feed. Chỉ lưu tiêu đề + đoạn trích ngắn của feed và link về bài gốc, KHÔNG lưu toàn văn (bản quyền).
/// </summary>
public class Article : BaseEntity
{
    [MaxLength(36)]
    public string SourceId { get; set; } = default!;

    public Source Source { get; set; } = default!;

    [MaxLength(500)]
    public string Title { get; set; } = default!;

    [MaxLength(2048)]
    public string Url { get; set; } = default!;

    /// <summary>SHA-256 (hex, 64 ký tự) của Url đã chuẩn hoá. Unique: chốt chặn để cùng một bài không bao giờ vào hai lần.</summary>
    [MaxLength(64)]
    public string UrlHash { get; set; } = default!;

    /// <summary>Đoạn trích của feed, đã bỏ HTML, cắt ngắn.</summary>
    [MaxLength(1000)]
    public string? Summary { get; set; }

    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    /// <summary>NewsCategory.* (sao chép từ nguồn để lọc nhanh, không cần join).</summary>
    [MaxLength(20)]
    public string Category { get; set; } = default!;

    public DateTimeOffset PublishedAt { get; set; }

    public DateTimeOffset FetchedAt { get; set; }

    /// <summary>Điểm quan trọng 0-100 tính lúc nạp tin bằng luật từ khoá (NewsRules.Classify), ưu tiên AI, lập trình và tin ảnh hưởng nghề dev. Chưa phải AI.</summary>
    public int Priority { get; set; }

    /// <summary>Chủ đề khớp, bọc dấu phẩy hai đầu để lọc khớp chính xác, ví dụ ",Ai,DevImpact,"; chuỗi rỗng nếu không có.</summary>
    [MaxLength(100)]
    public string Topics { get; set; } = "";
}

/// <summary>Chủ đề ưu tiên của người đọc (lưu dạng chuỗi trong cột Article.Topics).</summary>
public static class NewsTopic
{
    public const string Ai = "Ai";
    public const string Dev = "Dev";
    /// <summary>Tin công nghệ có thể ảnh hưởng tới nghề dev: sa thải, tuyển dụng, đổi giấy phép, lỗ hổng, công cụ AI thay thế việc...</summary>
    public const string DevImpact = "DevImpact";

    public static readonly string[] All = [Ai, Dev, DevImpact];
}
