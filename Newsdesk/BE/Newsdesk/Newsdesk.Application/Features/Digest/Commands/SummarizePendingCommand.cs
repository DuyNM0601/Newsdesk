using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Digest.Commands
{
    /// <summary>
    /// Tóm tắt các bài trong bản tin chưa có tóm tắt (ngày mới nhất trước, hạng cao trước), tối đa MaxCount bài và không quá Budget
    /// thời gian mỗi lần gọi, để cron không bị giữ lâu. Bài lỗi được thử lại ở nhịp sau cho tới DigestRules.MaxAttempts lần.
    /// Trả về số bài tóm tắt thành công.
    /// </summary>
    public sealed record SummarizePendingCommand(int MaxCount, TimeSpan Budget) : ICommand<int>;
}
