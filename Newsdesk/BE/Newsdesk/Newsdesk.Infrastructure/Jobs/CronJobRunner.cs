using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Newsdesk.Application.Common;
using Newsdesk.Domain.Entities;
using Newsdesk.Infrastructure.Data;

namespace Newsdesk.Infrastructure.Jobs
{
    /// <summary>
    /// Bộ chạy chung, và là job DUY NHẤT mà Quartz biết. Quartz đánh thức nó mỗi phút; nó đọc bảng CronJobs, job nào bật và đến hạn
    /// thì tạo class ICronJob tương ứng trong DI rồi gọi RunAsync, xong ghi kết quả lại vào bảng.
    /// DisallowConcurrentExecution: nhịp trước chưa xong thì nhịp sau không chạy chồng lên.
    /// </summary>
    [DisallowConcurrentExecution]
    public sealed class CronJobRunner(
        IServiceScopeFactory scopes,
        CronJobRegistry registry,
        TimeProvider clock,
        ILogger<CronJobRunner> logger) : IJob
    {
        public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken) =>
            await RunDueJobsAsync(cancellationToken);

        public async Task RunDueJobsAsync(CancellationToken ct)
        {
            await using AsyncServiceScope scope = scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DateTimeOffset now = clock.GetUtcNow();

            await EnsureRowsAsync(db, now, ct);

            // shortcut: các job đến hạn chạy lần lượt trong một nhịp; một job chạy lâu sẽ làm job khác trễ. Chạy song song khi có nhiều job nặng.
            List<CronJob> rows = await db.CronJobs.Where(x => x.Status == CronJobStatus.Active).ToListAsync(ct);
            foreach (CronJob row in rows)
            {
                if (!registry.Jobs.TryGetValue(row.Name, out var job))
                {
                    if (registry.ReportedUnknown.Add(row.Name))
                        logger.LogWarning("CronJobs row '{Name}' has no matching ICronJob class in code; skipped.", row.Name);
                    continue;
                }

                if (!TryGetNextRun(row, out DateTimeOffset? next))
                {
                    await RecordInvalidCronAsync(db, row, ct);
                    continue;
                }

                if (next is not null && next <= now)
                    await RunOneAsync(db, row, job.Type, now, ct);
            }
        }

        /// <summary>Tạo dòng cho job mới trong code (lịch lấy từ [CronSchedule]). Dòng đã có thì giữ nguyên, để lịch sửa trong database không bị ghi đè.</summary>
        private async Task EnsureRowsAsync(AppDbContext db, DateTimeOffset now, CancellationToken ct)
        {
            HashSet<string> existing = (await db.CronJobs.Select(x => x.Name).ToListAsync(ct)).ToHashSet();
            bool added = false;
            foreach (var (name, job) in registry.Jobs)
            {
                if (existing.Contains(name))
                    continue;
                db.CronJobs.Add(new CronJob { Name = name, CronExpression = job.DefaultCron, Status = CronJobStatus.Active, CreatedAt = now });
                added = true;
            }
            if (!added)
                return;

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Một bản API khác vừa tạo cùng dòng (unique Name). Bỏ, nhịp sau sẽ thấy dòng đó.
                db.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Lần chạy kế tiếp = giờ khớp cron đầu tiên SAU lần chạy gần nhất (chưa chạy lần nào thì sau lúc tạo dòng). Dùng mốc "lần chạy cuối"
        /// thay vì "đúng phút này" nên API tắt rồi bật lại vẫn chạy bù đúng một lần, không bỏ sót cũng không chạy dồn.
        /// </summary>
        private static bool TryGetNextRun(CronJob row, out DateTimeOffset? next)
        {
            next = null;
            if (!CronExpression.TryParse(row.CronExpression, out _))
                return false;

            // Tính theo UTC (mặc định của Quartz là múi giờ của máy chủ, không nên phụ thuộc vào đó).
            var expression = new CronExpression(row.CronExpression, TimeZoneInfo.Utc);
            next = expression.GetNextValidTimeAfter(row.LastRunAt ?? row.CreatedAt);
            return true;
        }

        private async Task RunOneAsync(AppDbContext db, CronJob row, Type jobType, DateTimeOffset now, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            string? error = null;
            try
            {
                // Mỗi lần chạy một DI scope riêng: job có DbContext/ISender riêng, sạch.
                await using AsyncServiceScope jobScope = scopes.CreateAsyncScope();
                var job = (ICronJob)jobScope.ServiceProvider.GetRequiredService(jobType);
                await job.RunAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Ranh giới với code của từng job: một job lỗi không được làm các job khác ngừng chạy. Ghi lại vào bảng để biết.
                error = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
                logger.LogError(ex, "Cron job {Name} failed.", row.Name);
            }

            row.LastRunAt = now;
            row.LastDurationMs = (int)stopwatch.ElapsedMilliseconds;
            row.LastRunStatus = error is null ? CronJobRunStatus.Success : CronJobRunStatus.Failed;
            row.LastError = error;
            await db.SaveChangesAsync(ct);
        }

        private async Task RecordInvalidCronAsync(AppDbContext db, CronJob row, CancellationToken ct)
        {
            const string message = "Invalid cron expression.";
            if (row.LastRunStatus == CronJobRunStatus.Failed && row.LastError == message)
                return; // đã ghi rồi, không ghi lại mỗi phút

            logger.LogError("Cron job {Name} has an invalid cron expression '{Cron}'; it will not run until fixed.", row.Name, row.CronExpression);
            row.LastRunStatus = CronJobRunStatus.Failed;
            row.LastError = message;
            await db.SaveChangesAsync(ct);
        }
    }
}
