using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.Repos.Models;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Repos.Queries
{
    public sealed class ListReposQueryHandler(IAppDbContext db, ILogger<ListReposQueryHandler> logger)
        : QueryHandler<ListReposQuery, IReadOnlyList<RepoModel>>(logger)
    {
        public override async Task<Result<IReadOnlyList<RepoModel>>> HandleAsync(ListReposQuery query, CancellationToken ct)
        {
            if (query.Limit is < 1 or > 50)
                return Fail(Error.Validation("limit", "Limit must be between 1 and 50."));

            List<Repo> repos = await db.Repos
                .AsNoTracking()
                .OrderByDescending(r => r.DiscoveredOn).ThenBy(r => r.Rank)
                .Take(query.Limit)
                .ToListAsync(ct);

            return Ok(repos.Select(r => new RepoModel
            {
                FullName = r.FullName,
                Url = r.Url,
                Description = r.Description,
                Language = r.Language,
                License = r.License,
                Stars = r.Stars,
                Kind = r.Kind,
                DiscoveredOn = r.DiscoveredOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Tldr = r.SummaryStatus == SummaryStatus.Success ? r.Tldr : null,
                KeyPoints = r.SummaryStatus == SummaryStatus.Success ? JsonSerializer.Deserialize<string[]>(r.KeyPoints) ?? [] : [],
                WhyItMatters = r.SummaryStatus == SummaryStatus.Success && !string.IsNullOrWhiteSpace(r.WhyItMatters) ? r.WhyItMatters : null,
                SummaryLanguage = r.SummaryStatus == SummaryStatus.Success ? r.SummaryLanguage : null,
            }).ToList());
        }
    }
}
