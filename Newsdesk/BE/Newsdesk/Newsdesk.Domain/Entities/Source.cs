using System.ComponentModel.DataAnnotations;

namespace Newsdesk.Domain.Entities;

/// <summary>Chủ đề của nguồn tin / bài viết, lưu dạng chuỗi (cột Category).</summary>
public static class NewsCategory
{
    public const string Tech = "Tech";
    public const string Programming = "Programming";
    public const string Politics = "Politics";
    public const string Economy = "Economy";

    public static readonly string[] All = [Tech, Programming, Politics, Economy];
}

/// <summary>Trạng thái bật/tắt của một nguồn tin, lưu dạng chuỗi trong cột Source.Status.</summary>
public static class SourceStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}

/// <summary>Kết quả lần tải feed cuối của một nguồn, lưu dạng chuỗi trong cột Source.LastFetchStatus.</summary>
public static class FetchStatus
{
    public const string Success = "Success";
    public const string Failed = "Failed";
}

/// <summary>
/// Một nguồn tin (RSS/Atom) trong danh sách trắng do người quản trị duyệt. Thêm/tắt nguồn chỉ cần sửa dòng trong database.
/// </summary>
public class Source : BaseEntity
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(500)]
    public string FeedUrl { get; set; } = default!;

    /// <summary>NewsCategory.*</summary>
    [MaxLength(20)]
    public string Category { get; set; } = default!;

    /// <summary>Mã ngôn ngữ của nguồn, ví dụ "en".</summary>
    [MaxLength(10)]
    public string Language { get; set; } = "en";

    /// <summary>SourceStatus.Active (được tải) / Inactive (tạm dừng).</summary>
    [MaxLength(20)]
    public string Status { get; set; } = SourceStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastFetchedAt { get; set; }

    /// <summary>FetchStatus.Success / Failed; null nếu chưa tải lần nào.</summary>
    [MaxLength(20)]
    public string? LastFetchStatus { get; set; }

    [MaxLength(500)]
    public string? LastError { get; set; }
}
