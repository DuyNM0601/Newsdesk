using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.News.Commands
{
    /// <summary>Tải feed của mọi nguồn Active (nhịp của cron). Trả về tổng số bài mới.</summary>
    public sealed record FetchAllFeedsCommand : ICommand<int>;
}
