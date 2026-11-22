using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Digest.Commands
{
    /// <summary>
    /// Chọn khoảng 50 bài mới và quan trọng nhất trong 24 giờ qua làm bản tin của ngày hôm nay (giờ Việt Nam) và lưu lại.
    /// Trả về số bài đã chọn; 0 nếu bản tin hôm nay đã có (idempotent) hoặc chưa có bài đủ điều kiện.
    /// OnlyIfNoDigestYet: chỉ tạo khi database CHƯA có bản tin nào (khởi động lần đầu), để không phải chờ tới giờ cron ngày hôm sau.
    /// </summary>
    public sealed record BuildDailyDigestCommand(bool OnlyIfNoDigestYet = false) : ICommand<int>;
}
