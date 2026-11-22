using MediatR;
using Microsoft.Extensions.Logging;
using Newsdesk.Application.Common;
using Newsdesk.Application.Features.Digest.Commands;
using Newsdesk.Application.Features.Repos.Commands;
using Newsdesk.Infrastructure.Ai;

namespace Newsdesk.Infrastructure.Jobs
{
    /// <summary>
    /// Cứ 3 phút tóm tắt dần: các bài trong bản tin (tối đa 12 bài / 150 giây) rồi các repo GitHub (tối đa 4 repo / 60 giây), để không giữ
    /// bộ chạy cron lâu. Model chạy trên máy chậm nên cả ngày mất vài chục phút; bản lỗi được thử lại ở nhịp sau.
    /// Khởi động lần đầu chưa có bản tin nào thì tạo luôn, khỏi chờ tới 06:00 hôm sau.
    /// </summary>
    [CronSchedule("0 */3 * ? * *")]
    public sealed class SummarizePendingJob(ISender sender, AiOptions ai, ILogger<SummarizePendingJob> logger) : ICronJob
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (ai.Provider == "None")
                return;

            await sender.Send(new BuildDailyDigestCommand(OnlyIfNoDigestYet: true), cancellationToken);

            var articles = await sender.Send(new SummarizePendingCommand(MaxCount: 12, Budget: TimeSpan.FromSeconds(150)), cancellationToken);
            var repos = await sender.Send(new SummarizePendingReposCommand(MaxCount: 4, Budget: TimeSpan.FromSeconds(60)), cancellationToken);

            if (!articles.IsSuccess || !repos.IsSuccess)
                throw new InvalidOperationException((articles.Error ?? repos.Error)?.Message ?? "Summarizing failed.");

            if (articles.Value + repos.Value > 0)
                logger.LogInformation("Summarized {Articles} article(s) and {Repos} repo(s).", articles.Value, repos.Value);
        }
    }
}
