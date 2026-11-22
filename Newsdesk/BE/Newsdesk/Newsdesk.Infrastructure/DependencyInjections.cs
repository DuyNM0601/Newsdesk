using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newsdesk.Application.Common;
using Newsdesk.Infrastructure.Auth;
using Quartz;
using Newsdesk.Infrastructure.Data;
using Newsdesk.Infrastructure.Jobs;
using Newsdesk.Infrastructure.News;
using Microsoft.EntityFrameworkCore;

namespace Newsdesk.Infrastructure
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>((servicesprovider, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            // Đọc thẳng từ configuration, không ép bắt buộc lúc khởi động: thiếu Jwt:Key / Google:ClientId
            // thì chỉ báo lỗi rõ ràng khi thật sự đăng nhập, để không chặn việc chạy API hay tạo migration.
            var jwtOptions = new JwtOptions();
            jwtOptions.Issuer = configuration["Jwt:Issuer"] ?? jwtOptions.Issuer;
            jwtOptions.Audience = configuration["Jwt:Audience"] ?? jwtOptions.Audience;
            jwtOptions.Key = configuration["Jwt:Key"] ?? jwtOptions.Key;
            if (int.TryParse(configuration["Jwt:ExpiresMinutes"], out int expiresMinutes))
                jwtOptions.ExpiresMinutes = expiresMinutes;
            if (int.TryParse(configuration["Jwt:RefreshExpiresDays"], out int refreshExpiresDays))
                jwtOptions.RefreshExpiresDays = refreshExpiresDays;
            services.AddSingleton(jwtOptions);

            var googleOptions = new GoogleOptions { ClientId = configuration["Google:ClientId"] ?? "" };
            services.AddSingleton(googleOptions);

            services.AddSingleton(TimeProvider.System);

            // Đọc feed RSS/Atom. HttpClient dùng chung một thể hiện; một số nguồn từ chối request không có User-Agent.
            // Giới hạn 5MB mỗi feed và 20 giây mỗi lần tải để một nguồn lỗi không kéo cả nhịp.
            services.AddSingleton<IFeedFetcher>(_ =>
            {
                var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20), MaxResponseContentBufferSize = 5 * 1024 * 1024 };
                http.DefaultRequestHeaders.UserAgent.ParseAdd("FeedReader/1.0");
                http.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/atom+xml, application/xml;q=0.9, text/xml;q=0.8, */*;q=0.5");
                return new RssFeedFetcher(http);
            });

            // Cron: khai báo MỘT LẦN ở đây, không phải mỗi job một lần.
            //  - AddCronJobs: quét mọi class ICronJob (kèm [CronSchedule]) trong assembly này, đăng ký vào DI. Job mới chỉ cần viết class.
            //  - Quartz chỉ biết đúng một job là CronJobRunner, chạy mỗi phút (giây 0). Runner đọc bảng CronJobs rồi gọi job nào đến hạn.
            // Trình lưu job của Quartz là RAM: runner không giữ trạng thái, lịch và kết quả nằm ở bảng CronJobs nên khởi động lại không mất gì.
            // MisfireDoNothing: lỡ nhịp thì bỏ; runner tự chạy bù đúng một lần ở nhịp sau nhờ mốc LastRunAt.
            services.AddCronJobs(typeof(DependencyInjections).Assembly);
            services.AddQuartz(q =>
            {
                var runnerKey = new JobKey(nameof(CronJobRunner));
                q.AddJob<CronJobRunner>(o => o.WithIdentity(runnerKey));
                q.AddTrigger(t => t
                    .ForJob(runnerKey)
                    .WithIdentity($"{nameof(CronJobRunner)}-trigger")
                    .WithCronSchedule("0 * * ? * *", c => c.WithMisfireInstruction(CronTriggerMisfireInstruction.DoNothing)));
            });
            services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();
            return services;
        }
    }
}
