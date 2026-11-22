using MediatR;
using Newsdesk.API.Shared;
using Newsdesk.Application.Features.Digest.Models;
using Newsdesk.Application.Features.Digest.Queries;

namespace Newsdesk.API.Features.Digest
{
    public static class DigestEndpoints
    {
        /// <summary>Đọc bản tin là công khai. Bản tóm tắt đã được tạo sẵn bằng cron, không gọi AI trong request.</summary>
        public static void MapDigestApi(this IEndpointRouteBuilder endpoints)
        {
            // date: yyyy-MM-dd (giờ Việt Nam); bỏ trống thì lấy bản tin mới nhất.
            endpoints.MapGet("/digest", async (string? date, ISender sender) =>
                (await sender.Send(new GetDigestQuery(date))).ToHttpResult())
            .Produces<DigestModel>()
            .ProducesValidationProblem();

            endpoints.MapGet("/digest/dates", async (ISender sender) =>
                (await sender.Send(new ListDigestDatesQuery())).ToHttpResult())
            .Produces<IReadOnlyList<string>>();
        }
    }
}
