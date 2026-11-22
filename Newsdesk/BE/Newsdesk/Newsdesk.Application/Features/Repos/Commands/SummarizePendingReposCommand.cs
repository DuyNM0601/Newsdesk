using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Repos.Commands
{
    /// <summary>
    /// AI tóm tắt "repo này làm gì" cho các repo chưa có tóm tắt (ngày mới nhất, hạng cao trước), tối đa MaxCount repo và không quá Budget
    /// thời gian. Xoá đoạn README tạm sau khi tóm tắt thành công. Trả về số repo tóm tắt được.
    /// </summary>
    public sealed record SummarizePendingReposCommand(int MaxCount, TimeSpan Budget) : ICommand<int>;
}
