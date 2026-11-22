using MediatR;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.News.Commands;

namespace Newsdesk.Infrastructure.Jobs
{
    /// <summary>Mỗi 15 phút tải feed của mọi nguồn Active (lịch có thể đổi trong bảng CronJobs).</summary>
    [CronSchedule("0 */15 * ? * *")]
    public sealed class FetchAllFeedsJob(ISender sender, ILogger<FetchAllFeedsJob> logger) : ICronJob
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new FetchAllFeedsCommand(), cancellationToken);

            // Ném để CronJobRunner ghi LastRunStatus = Failed + LastError vào bảng CronJobs.
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error?.Message ?? "Fetching feeds failed.");

            logger.LogInformation("Feed fetch added {Count} new article(s).", result.Value);
        }
    }
}
