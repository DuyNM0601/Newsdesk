namespace Newsdesk.Domain.Entities;

/// <summary>
/// Một bài trong bản tin của một ngày. Mỗi ngày cron chọn khoảng 50 bài mới và quan trọng nhất rồi lưu lại ở đây
/// (giữ lịch sử để người đọc xem lại các ngày trước). Một bài chỉ vào một bản tin.
/// </summary>
public class DigestEntry : BaseEntity
{
    /// <summary>Ngày của bản tin theo giờ Việt Nam (UTC+7).</summary>
    public DateOnly DigestDate { get; set; }

    /// <summary>Thứ hạng trong ngày, 1 là quan trọng nhất.</summary>
    public int Rank { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(36)]
    public string ArticleId { get; set; } = default!;

    public Article Article { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}
