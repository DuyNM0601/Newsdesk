using MediatR;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.Digest.Commands;

namespace Newsdesk.Infrastructure.Jobs
{
    /// <summary>
    /// Mỗi ngày lúc 06:00 giờ Việt Nam (23:00 UTC) chọn khoảng 50 bài mới và quan trọng nhất của 24 giờ qua và lưu thành bản tin.
    /// Chỉ chọn bài, việc tóm tắt do SummarizePendingJob làm dần sau đó. Lịch đổi được trong bảng CronJobs.
    /// </summary>
    [CronSchedule("0 0 23 * * ?")]
    public sealed class BuildDailyDigestJob(ISender sender, ILogger<BuildDailyDigestJob> logger) : ICronJob
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new BuildDailyDigestCommand(), cancellationToken);
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error?.Message ?? "Building the daily digest failed.");

            logger.LogInformation("Daily digest selected {Count} article(s).", result.Value);
        }
    }
}
