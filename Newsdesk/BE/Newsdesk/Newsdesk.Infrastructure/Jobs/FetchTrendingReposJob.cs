using MediatR;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.Repos.Commands;

namespace Newsdesk.Infrastructure.Jobs
{
    /// <summary>
    /// Mỗi ngày lúc 05:30 giờ Việt Nam (22:30 UTC, trước bản tin 06:00) chọn khoảng 12 repo GitHub đang lên (AI trước) và lấy README.
    /// Tốn đúng một request tìm kiếm + một request README mỗi repo. Việc AI tóm tắt do SummarizePendingJob làm dần.
    /// </summary>
    [CronSchedule("0 30 22 * * ?")]
    public sealed class FetchTrendingReposJob(ISender sender, ILogger<FetchTrendingReposJob> logger) : ICronJob
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new FetchTrendingReposCommand(), cancellationToken);
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error?.Message ?? "Fetching trending repos failed.");

            logger.LogInformation("Trending repos: picked {Count}.", result.Value);
        }
    }
}
