using MediatR;
using Newsdesk.API.Shared;
using Newsdesk.Application.Features.News.Models;
using Newsdesk.Application.Features.News.Queries;

namespace Newsdesk.API.Features.News
{
    public static class NewsEndpoints
    {
        /// <summary>Đọc tin là công khai (không cần đăng nhập).</summary>
        public static void MapNewsApi(this IEndpointRouteBuilder endpoints)
        {
            // topic: Ai | Dev | DevImpact. important=true: chỉ bài quan trọng. sort: new | top (quan trọng nhất trong 72 giờ).
            endpoints.MapGet("/news", async (string? category, string? topic, string? q, bool? important, string? sort, int? page, int? pageSize, ISender sender) =>
                (await sender.Send(new ListArticlesQuery(category, q, page ?? 1, pageSize ?? 20, topic, important ?? false, sort ?? "new"))).ToHttpResult())
            .Produces<ArticlePageModel>()
            .ProducesValidationProblem();

            endpoints.MapGet("/sources", async (ISender sender) =>
                (await sender.Send(new ListSourcesQuery())).ToHttpResult())
            .Produces<IReadOnlyList<SourceModel>>();
        }
    }
}
