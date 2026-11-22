using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.News.Models;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.News.Queries
{
    public sealed class ListSourcesQueryHandler(IAppDbContext db, ILogger<ListSourcesQueryHandler> logger)
        : QueryHandler<ListSourcesQuery, IReadOnlyList<SourceModel>>(logger)
    {
        public override async Task<Result<IReadOnlyList<SourceModel>>> HandleAsync(ListSourcesQuery query, CancellationToken ct)
        {
            List<Source> sources = await db.Sources
                .AsNoTracking()
                .Where(x => x.Status == SourceStatus.Active)
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .ToListAsync(ct);

            return Ok(sources.Select(SourceModel.From).ToList());
        }
    }
}
