using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newsdesk.Application.Common;
using Microsoft.Extensions.Logging;
using Newsdesk.Infrastructure.Ai;
using Newsdesk.Infrastructure.Auth;
using Newsdesk.Infrastructure.GitHub;
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

            // GitHub API công khai (miễn phí). Token chỉ-đọc, KHÔNG cần quyền nào, là tuỳ chọn nhưng nên có: không token thì hạn mức tính theo
            // địa chỉ IP (rất thấp, hay hết); có token thì 5000 request/giờ. Đặt bằng user-secrets: dotnet user-secrets set "GitHub:Token" "<token>".
            string? gitHubToken = configuration["GitHub:Token"];
            services.AddSingleton<IGitHubClient>(_ =>
            {
                var http = new HttpClient { BaseAddress = new Uri(configuration["GitHub:BaseUrl"] ?? "https://api.github.com"), Timeout = TimeSpan.FromSeconds(30), MaxResponseContentBufferSize = 2 * 1024 * 1024 };
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Newsdesk-Reader");
                http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
                http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
                if (!string.IsNullOrWhiteSpace(gitHubToken))
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gitHubToken);
                return new GitHubClient(http);
            });

            // AI tóm tắt: chọn nhà cung cấp bằng cấu hình Ai:Provider ("Ollama" mặc định, hoặc "None" để tắt); sai tên thì báo lỗi ngay lúc khởi động.
            // Ollama chưa cài/chưa chạy không chặn app khởi động: job tóm tắt chỉ ghi log cảnh báo và thử lại ở nhịp sau.
            var aiOptions = new AiOptions
            {
                Provider = configuration["Ai:Provider"] ?? "Ollama",
                Language = configuration["Ai:Language"] ?? "en",
                OllamaBaseUrl = configuration["Ai:Ollama:BaseUrl"] ?? new AiOptions().OllamaBaseUrl,
                OllamaModel = configuration["Ai:Ollama:Model"] ?? new AiOptions().OllamaModel,
            };
            if (int.TryParse(configuration["Ai:Ollama:TimeoutSeconds"], out int aiTimeout))
                aiOptions.TimeoutSeconds = aiTimeout;
            if (aiOptions.Language is not ("en" or "vi"))
                throw new InvalidOperationException($"Ai:Language must be \"en\" or \"vi\", but is \"{aiOptions.Language}\".");
            services.AddSingleton(aiOptions);
            switch (aiOptions.Provider)
            {
                case "Ollama":
                    services.AddSingleton<IArticleSummarizer>(sp => new OllamaArticleSummarizer(
                        new HttpClient { BaseAddress = new Uri(aiOptions.OllamaBaseUrl), Timeout = TimeSpan.FromSeconds(aiOptions.TimeoutSeconds) },
                        aiOptions,
                        sp.GetRequiredService<ILogger<OllamaArticleSummarizer>>()));
                    break;
                case "None":
                    services.AddSingleton<IArticleSummarizer, DisabledArticleSummarizer>();
                    break;
                default:
                    throw new InvalidOperationException($"Ai:Provider must be \"Ollama\" or \"None\", but is \"{aiOptions.Provider}\".");
            }

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
