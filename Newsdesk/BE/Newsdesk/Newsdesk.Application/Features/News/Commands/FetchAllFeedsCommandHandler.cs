using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Shared;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.News.Commands
{
    public sealed class FetchAllFeedsCommandHandler(
        IAppDbContext db,
        ISender sender,
        ILogger<FetchAllFeedsCommandHandler> logger)
        : CommandHandler<FetchAllFeedsCommand, int>(logger)
    {
        public override async Task<Result<int>> HandleAsync(FetchAllFeedsCommand command, CancellationToken ct)
        {
            List<string> sourceIds = await db.Sources
                .AsNoTracking()
                .Where(x => x.Status == SourceStatus.Active)
                .Select(x => x.Id)
                .ToListAsync(ct);

            // shortcut: tải tuần tự từng nguồn trong cùng một tiến trình. Giai đoạn RabbitMQ sẽ đổi thành "mỗi nguồn một message".
            int total = 0;
            foreach (string id in sourceIds)
            {
                Result<int> result = await sender.Send(new FetchSourceCommand(id), ct);
                if (result.IsSuccess)
                    total += result.Value;
            }

            return Ok(total);
        }
    }
}
