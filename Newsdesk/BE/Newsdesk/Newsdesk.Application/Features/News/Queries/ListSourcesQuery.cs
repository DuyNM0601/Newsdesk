using Newsdesk.Application.Features.News.Models;
using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.News.Queries
{
    /// <summary>Các nguồn tin đang được theo dõi (Active).</summary>
    public sealed record ListSourcesQuery : IQuery<IReadOnlyList<SourceModel>>;
}
