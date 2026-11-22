using Newsdesk.Application.Features.Repos.Models;
using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Repos.Queries
{
    /// <summary>Các repo mới giới thiệu gần nhất (ngày mới nhất trước, hạng cao trước), tối đa Limit (1-50).</summary>
    public sealed record ListReposQuery(int Limit = 24) : IQuery<IReadOnlyList<RepoModel>>;
}
