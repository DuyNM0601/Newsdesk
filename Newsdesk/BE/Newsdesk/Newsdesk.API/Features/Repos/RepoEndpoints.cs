using MediatR;
using Newsdesk.API.Shared;
using Newsdesk.Application.Features.Repos.Models;
using Newsdesk.Application.Features.Repos.Queries;

namespace Newsdesk.API.Features.Repos
{
    public static class RepoEndpoints
    {
        /// <summary>Repo GitHub đang lên kèm tóm tắt AI. Công khai, đọc từ dữ liệu đã lưu, không gọi GitHub hay AI trong request.</summary>
        public static void MapRepoApi(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/repos", async (int? limit, ISender sender) =>
                (await sender.Send(new ListReposQuery(limit ?? 24))).ToHttpResult())
            .Produces<IReadOnlyList<RepoModel>>()
            .ProducesValidationProblem();
        }
    }
}
