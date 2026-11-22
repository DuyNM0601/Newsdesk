using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.News.Commands
{
    /// <summary>Tải feed của MỘT nguồn và lưu bài mới. Trả về số bài mới. Lỗi tải feed được ghi vào nguồn, không phải lỗi của command.</summary>
    public sealed record FetchSourceCommand(string SourceId) : ICommand<int>;
}
