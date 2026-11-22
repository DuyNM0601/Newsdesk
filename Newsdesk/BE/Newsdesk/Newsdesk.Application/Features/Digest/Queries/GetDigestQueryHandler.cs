using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.Digest.Models;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Digest.Queries
{
    public sealed class GetDigestQueryHandler(IAppDbContext db, ILogger<GetDigestQueryHandler> logger)
        : QueryHandler<GetDigestQuery, DigestModel>(logger)
    {
        public override async Task<Result<DigestModel>> HandleAsync(GetDigestQuery query, CancellationToken ct)
        {
            DateOnly date;
            if (query.Date is null)
            {
                DateOnly? latest = await db.DigestEntries.MaxAsync(x => (DateOnly?)x.DigestDate, ct);
                if (latest is null)
                    return Ok(new DigestModel());
                date = latest.Value;
            }
            else if (!DateOnly.TryParseExact(query.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return Fail(Error.Validation("date", "Date must be in the form yyyy-MM-dd."));
            }

            // Trái (left join): bài chưa có tóm tắt vẫn hiện trong bản tin, chỉ thiếu phần tóm tắt.
            var rows = await (
                from e in db.DigestEntries.AsNoTracking()
                where e.DigestDate == date
                join a in db.Articles on e.ArticleId equals a.Id
                join s in db.ArticleSummaries.Where(x => x.Status == SummaryStatus.Success) on a.Id equals s.ArticleId into summaries
                from s in summaries.DefaultIfEmpty()
                orderby e.Rank
                select new
                {
                    e.Rank,
                    ArticleId = a.Id,
                    a.Title,
                    a.Url,
                    a.Category,
                    SourceName = a.Source.Name,
                    a.PublishedAt,
                    a.ImageUrl,
                    a.Priority,
                    a.Topics,
                    Tldr = s != null ? s.Tldr : null,
                    KeyPoints = s != null ? s.KeyPoints : null,
                    WhyItMatters = s != null ? s.WhyItMatters : null,
                    Language = s != null ? s.Language : null,
                })
                .ToListAsync(ct);

            List<DigestItemModel> items = rows.Select(r => new DigestItemModel
            {
                Rank = r.Rank,
                ArticleId = r.ArticleId,
                Title = r.Title,
                Url = r.Url,
                Category = r.Category,
                SourceName = r.SourceName,
                PublishedAt = r.PublishedAt,
                ImageUrl = r.ImageUrl,
                Priority = r.Priority,
                Topics = r.Topics.Split(',', StringSplitOptions.RemoveEmptyEntries),
                Tldr = r.Tldr,
                KeyPoints = r.KeyPoints is null ? [] : JsonSerializer.Deserialize<string[]>(r.KeyPoints) ?? [],
                WhyItMatters = string.IsNullOrWhiteSpace(r.WhyItMatters) ? null : r.WhyItMatters,
                Language = r.Language,
            }).ToList();

            return Ok(new DigestModel
            {
                Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Total = items.Count,
                Summarized = items.Count(i => i.Tldr is not null),
                Items = items,
            });
        }
    }
}
