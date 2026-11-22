using System.ComponentModel.DataAnnotations;

namespace Newsdesk.Domain.Entities;

/// <summary>Trạng thái bật/tắt của một cron job, lưu dạng chuỗi trong cột CronJob.Status.</summary>
public static class CronJobStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}

/// <summary>Kết quả lần chạy cuối của một cron job, lưu dạng chuỗi trong cột CronJob.LastRunStatus.</summary>
public static class CronJobRunStatus
{
    public const string Success = "Success";
    public const string Failed = "Failed";
}

/// <summary>
/// Một dòng cho mỗi cron job trong code: lịch chạy, bật/tắt và kết quả lần chạy cuối. Đổi lịch hoặc bật/tắt chỉ cần sửa dòng này
/// trong database, không cần deploy. Dòng được tự tạo khi API khởi động lần đầu thấy job mới trong code.
/// </summary>
public class CronJob : BaseEntity
{
    /// <summary>Tên class của job (unique), ví dụ "DispatchDueRemindersJob".</summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    /// <summary>Biểu thức cron kiểu Quartz (7 trường, có giây), tính theo UTC. Ví dụ "0 * * ? * *" = mỗi phút, "0 0 3 * * ?" = 03:00 mỗi ngày.</summary>
    [MaxLength(100)]
    public string CronExpression { get; set; } = default!;

    /// <summary>CronJobStatus.Active (cron sẽ chạy) / Inactive (tạm dừng).</summary>
    [MaxLength(20)]
    public string Status { get; set; } = CronJobStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Lần chạy gần nhất (kể cả lỗi). Cũng là mốc để tính lần chạy kế tiếp.</summary>
    public DateTimeOffset? LastRunAt { get; set; }

    public int? LastDurationMs { get; set; }

    /// <summary>CronJobRunStatus.Success / Failed; null nếu chưa chạy lần nào.</summary>
    [MaxLength(20)]
    public string? LastRunStatus { get; set; }

    [MaxLength(500)]
    public string? LastError { get; set; }
}
