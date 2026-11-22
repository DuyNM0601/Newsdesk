using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newsdesk.Application.Common;

namespace Newsdesk.Infrastructure.Jobs
{
    /// <summary>Danh sách job có trong code (tên → class + lịch mặc định), dựng một lần lúc khởi động bằng cách quét các class ICronJob.</summary>
    public sealed class CronJobRegistry
    {
        public IReadOnlyDictionary<string, (Type Type, string DefaultCron)> Jobs { get; }

        /// <summary>Tên job có dòng trong database nhưng không còn class trong code, đã báo cảnh báo rồi (mỗi tên một lần).</summary>
        public HashSet<string> ReportedUnknown { get; } = [];

        public CronJobRegistry(IEnumerable<Type> jobTypes)
        {
            var jobs = new Dictionary<string, (Type, string)>();
            foreach (Type type in jobTypes)
            {
                // Fail-fast lúc khởi động: thiếu lịch hoặc trùng tên thì báo ngay, không chờ tới lúc job lẽ ra phải chạy.
                var schedule = type.GetCustomAttribute<CronScheduleAttribute>()
                    ?? throw new InvalidOperationException($"{type.Name} implements ICronJob but has no [CronSchedule(\"...\")] attribute.");
                if (!jobs.TryAdd(type.Name, (type, schedule.Expression)))
                    throw new InvalidOperationException($"Two cron jobs are named '{type.Name}'; class names must be unique.");
            }
            Jobs = jobs;
        }
    }

    public static class CronJobServiceCollectionExtensions
    {
        /// <summary>Quét các assembly, đăng ký mọi class ICronJob vào DI (scoped) và dựng CronJobRegistry.</summary>
        public static IServiceCollection AddCronJobs(this IServiceCollection services, params Assembly[] assemblies)
        {
            List<Type> jobTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICronJob).IsAssignableFrom(t))
                .ToList();

            foreach (Type type in jobTypes)
                services.AddScoped(type);
            services.AddSingleton(new CronJobRegistry(jobTypes));
            return services;
        }
    }
}
