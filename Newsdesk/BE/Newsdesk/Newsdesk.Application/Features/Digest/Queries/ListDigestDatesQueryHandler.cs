using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Digest.Queries
{
    public sealed class ListDigestDatesQueryHandler(IAppDbContext db, ILogger<ListDigestDatesQueryHandler> logger)
        : QueryHandler<ListDigestDatesQuery, IReadOnlyList<string>>(logger)
    {
        public override async Task<Result<IReadOnlyList<string>>> HandleAsync(ListDigestDatesQuery query, CancellationToken ct)
        {
            List<DateOnly> dates = await db.DigestEntries
                .AsNoTracking()
                .Select(x => x.DigestDate)
                .Distinct()
                .OrderByDescending(d => d)
                .Take(60)
                .ToListAsync(ct);

            return Ok(dates.Select(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).ToList());
        }
    }
}
