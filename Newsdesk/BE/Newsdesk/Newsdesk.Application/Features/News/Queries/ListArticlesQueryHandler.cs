using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.News.Models;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.News.Queries
{
    public sealed class ListArticlesQueryHandler(IAppDbContext db, TimeProvider clock, ILogger<ListArticlesQueryHandler> logger)
        : QueryHandler<ListArticlesQuery, ArticlePageModel>(logger)
    {
        public override async Task<Result<ArticlePageModel>> HandleAsync(ListArticlesQuery query, CancellationToken ct)
        {
            if (query.Category is not null && !NewsCategory.All.Contains(query.Category))
                return Fail(Error.Validation("category", $"Category must be one of: {string.Join(", ", NewsCategory.All)}."));
            if (query.Topic is not null && !NewsTopic.All.Contains(query.Topic))
                return Fail(Error.Validation("topic", $"Topic must be one of: {string.Join(", ", NewsTopic.All)}."));
            if (query.Sort is not ("new" or "top"))
                return Fail(Error.Validation("sort", "Sort must be \"new\" or \"top\"."));
            if (query.Page < 1)
                return Fail(Error.Validation("page", "Page must be at least 1."));
            if (query.PageSize is < 1 or > NewsRules.MaxPageSize)
                return Fail(Error.Validation("pageSize", $"PageSize must be between 1 and {NewsRules.MaxPageSize}."));

            IQueryable<Article> articles = db.Articles.AsNoTracking();
            if (query.Category is not null)
                articles = articles.Where(x => x.Category == query.Category);
            if (query.Topic is not null)
            {
                string token = $",{query.Topic},"; // cột Topics bọc dấu phẩy nên khớp chính xác ("Dev" không khớp "DevImpact")
                articles = articles.Where(x => x.Topics.Contains(token));
            }
            if (query.Important)
                articles = articles.Where(x => x.Priority >= NewsRules.ImportantThreshold);
            if (!string.IsNullOrWhiteSpace(query.Search))
                articles = articles.Where(x => x.Title.Contains(query.Search.Trim()));

            IOrderedQueryable<Article> ordered;
            if (query.Sort == "top")
            {
                DateTimeOffset since = clock.GetUtcNow() - NewsRules.TopWindow;
                ordered = articles.Where(x => x.PublishedAt >= since).OrderByDescending(x => x.Priority).ThenByDescending(x => x.PublishedAt);
            }
            else
            {
                ordered = articles.OrderByDescending(x => x.PublishedAt);
            }

            // Lấy dư 1 dòng để biết còn trang sau hay không, khỏi phải đếm tổng. Id làm khoá phụ cho thứ tự ổn định.
            List<ArticleModel> rows = await ordered
                .ThenBy(x => x.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize + 1)
                .Select(x => new ArticleModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Url = x.Url,
                    Summary = x.Summary,
                    ImageUrl = x.ImageUrl,
                    Category = x.Category,
                    SourceName = x.Source.Name,
                    PublishedAt = x.PublishedAt,
                    Priority = x.Priority,
                    TopicsRaw = x.Topics,
                })
                .ToListAsync(ct);

            List<ArticleModel> items = rows.Take(query.PageSize).ToList();
            foreach (ArticleModel item in items)
                item.Topics = item.TopicsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries);

            return Ok(new ArticlePageModel
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                HasMore = rows.Count > query.PageSize,
            });
        }
    }
}
