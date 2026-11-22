using Newsdesk.Application.Features.News.Models;
using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.News.Queries
{
    /// <summary>
    /// Danh sách bài, phân trang. Category: NewsCategory. Topic: NewsTopic (Ai/Dev/DevImpact). Important: chỉ bài từ ngưỡng quan trọng.
    /// Sort: "new" (mới nhất, mặc định) hoặc "top" (quan trọng nhất trong 72 giờ gần đây, đồng điểm thì mới hơn).
    /// </summary>
    public sealed record ListArticlesQuery(
        string? Category,
        string? Search,
        int Page = 1,
        int PageSize = 20,
        string? Topic = null,
        bool Important = false,
        string Sort = "new") : IQuery<ArticlePageModel>;
}
