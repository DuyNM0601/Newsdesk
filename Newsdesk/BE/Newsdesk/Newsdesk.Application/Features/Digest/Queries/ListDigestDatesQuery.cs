using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Digest.Queries
{
    /// <summary>Các ngày đã có bản tin (yyyy-MM-dd), mới nhất trước, tối đa 60 ngày.</summary>
    public sealed record ListDigestDatesQuery : IQuery<IReadOnlyList<string>>;
}
