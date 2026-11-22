using System.ComponentModel.DataAnnotations;

namespace Newsdesk.Domain.Entities;

/// <summary>Loại repo, lưu dạng chuỗi trong cột Repo.Kind.</summary>
public static class RepoKind
{
    public const string Ai = "Ai";
    public const string Other = "Other";
}

/// <summary>
/// Một repo GitHub "đang lên" (mới tạo gần đây mà nhiều sao), kèm bản tóm tắt AI "repo này làm gì". Chỉ lưu những gì cần để hiển thị
/// và tóm tắt; README chỉ giữ tạm một đoạn ngắn làm đầu vào cho AI rồi xoá sau khi tóm tắt xong.
/// </summary>
public class Repo : BaseEntity
{
    /// <summary>"chủ/tên", unique: mỗi repo chỉ được giới thiệu một lần.</summary>
    [MaxLength(200)]
    public string FullName { get; set; } = default!;

    [MaxLength(300)]
    public string Url { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    /// <summary>Mã giấy phép SPDX (MIT, Apache-2.0...); null nếu repo không khai báo.</summary>
    [MaxLength(50)]
    public string? License { get; set; }

    /// <summary>Chủ đề GitHub, cách nhau dấu phẩy.</summary>
    [MaxLength(300)]
    public string Topics { get; set; } = "";

    /// <summary>Số sao lúc phát hiện.</summary>
    public int Stars { get; set; }

    /// <summary>RepoKind.Ai nếu liên quan tới AI (luật từ khoá trên tên, mô tả, chủ đề), còn lại RepoKind.Other. AI được xếp lên trước.</summary>
    [MaxLength(20)]
    public string Kind { get; set; } = RepoKind.Other;

    public DateTimeOffset CreatedOnGitHub { get; set; }

    /// <summary>Ngày giới thiệu (giờ Việt Nam) và thứ hạng trong ngày, 1 là đầu.</summary>
    public DateOnly DiscoveredOn { get; set; }

    public int Rank { get; set; }

    /// <summary>Đầu README đã làm sạch, chỉ để đưa cho AI. Xoá (null) sau khi tóm tắt thành công.</summary>
    [MaxLength(2000)]
    public string? ReadmeExcerpt { get; set; }

    /// <summary>SummaryStatus.Success / Failed; null = chưa thử.</summary>
    [MaxLength(20)]
    public string? SummaryStatus { get; set; }

    [MaxLength(600)]
    public string Tldr { get; set; } = "";

    /// <summary>Mảng JSON các ý chính.</summary>
    [MaxLength(2000)]
    public string KeyPoints { get; set; } = "[]";

    /// <summary>Dành cho ai / dùng khi nào.</summary>
    [MaxLength(400)]
    public string WhyItMatters { get; set; } = "";

    [MaxLength(10)]
    public string SummaryLanguage { get; set; } = "";

    [MaxLength(100)]
    public string Model { get; set; } = "";

    [MaxLength(20)]
    public string PromptVersion { get; set; } = "";

    public int Attempts { get; set; }

    [MaxLength(500)]
    public string? Error { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
