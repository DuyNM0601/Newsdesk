using Newsdesk.Application.Features.Digest.Models;
using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Digest.Queries
{
    /// <summary>Bản tin của ngày Date (yyyy-MM-dd); bỏ trống thì lấy bản tin mới nhất.</summary>
    public sealed record GetDigestQuery(string? Date) : IQuery<DigestModel>;
}
