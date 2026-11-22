# CLAUDE.md — Newsdesk

Người dùng nói chuyện bằng tiếng Việt. Trả lời, comment giải thích và copy giao diện bằng tiếng Việt. Tên class, biến, endpoint và event dùng tiếng Anh.

## 1. Tổng quan

- Dự án: **Newsdesk**, web công khai cho nhiều người dùng, tổng hợp tin mới và quan trọng về công nghệ, lập trình, chính trị, kinh tế từ các nguồn quốc tế chính thống (RSS), kèm danh sách khoá học lập trình chọn lọc. Người đọc không cần đăng nhập; đăng nhập Google dùng cho tính năng cá nhân hoá về sau.
- Tác giả: Nguyễn Mạnh Duy. **Mục đích chính là học Clean Architecture + CQRS (+ Redis, RabbitMQ, Docker, Nginx) để đi phỏng vấn**, nhưng web dự định chạy công khai.
- Dự án đổi hướng từ "Streak" (nhắc nhở kỷ luật, gọi điện) sang Newsdesk vào 22/11/2026. Mọi thứ liên quan nhắc nhở / gọi điện đã bị xoá. Thư mục gốc `Desktop\Streak` do người dùng tự đổi tên; remote GitHub vẫn là `DuyNM0601/Streak` cho tới khi họ đổi.
- **Chỉ lưu tiêu đề + đoạn trích của feed + link về bài gốc, KHÔNG lưu toàn văn bài** (bản quyền).

### Lộ trình (giai đoạn)

Đã xong: **GitHub đang lên** (repo mới nhiều sao + AI tóm tắt, xem mục "GitHub đang lên"), **6** AI tóm tắt + bản tin hằng ngày (xem mục "AI tóm tắt và bản tin"), **0** dọn dẹp, **2** tin tức (entity `Source`/`Article`, tải RSS bằng cron, `GET /news`, `GET /sources`), đổi tên sang Newsdesk, **5** FE tin tức hướng "Báo in hiện đại" (trang nhất xếp theo độ quan trọng, trang mục + tìm kiếm), xếp hạng bằng từ khoá (`Article.Priority`/`Topics`).
Còn lại, theo thứ tự: **3** RabbitMQ (queue tải feed + retry + dead-letter), **4** Redis cache + rate limiting, **7** khoá học + trang Khoá học, **8** Docker + Nginx (+ tách Worker, CI), `User.Role` + endpoint quản trị nguồn.

AI tóm tắt chạy NỀN bằng cron, lưu vào DB, **không bao giờ gọi AI trong request của người đọc**. Xem mục "AI tóm tắt và bản tin" ở mục 3.

## 2. Công nghệ

- .NET 10, C#, ASP.NET Core **Minimal API** (không dùng Controller)
- EF Core 10 + SQL Server
- **MediatR 12.0.1** cho CQRS — ghim đúng bản này, vì từ bản 13.0 MediatR chuyển sang licence kép (RPL / thương mại). 12.0.1 là bản Apache 2.0 thuần.
- **Quartz.NET 4.7.3** (`Quartz.Extensions.Hosting`, ở Infrastructure) làm cron.
- Đọc RSS/Atom bằng `System.Xml.Linq` (không thêm package).
- Chưa dùng FluentValidation (input được kiểm trong handler, trả `Fail(field, message)`).
- Auth: `Google.Apis.Auth`, `Microsoft.IdentityModel.JsonWebTokens` (Infrastructure, ký JWT), `Microsoft.AspNetCore.Authentication.JwtBearer` (API).
- `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore` (giao diện thử API tại `/scalar`, chỉ ở Development). Không dùng Swashbuckle.
- Dự kiến thêm (hỏi người dùng trước khi cài): `RabbitMQ.Client` 7.x (không dùng MassTransit vì đã thương mại), `Microsoft.Extensions.Caching.StackExchangeRedis`.

## 3. Kiến trúc: Clean Architecture + Vertical Slice + CQRS/MediatR

4 project .NET (thư mục BE: `Newsdesk/BE/Newsdesk/`). **Không dùng Repository pattern** — Application gọi qua `IAppDbContext`.

```
Newsdesk/BE/Newsdesk/
  Newsdesk.slnx
  Newsdesk.Domain/
    Entities/BaseEntity, IAuditable, ISoftDelete, User, RefreshToken, Source, Article (+NewsTopic), ArticleSummary, DigestEntry, Repo (+RepoKind), CronJob   # không biết EF Core
  Newsdesk.Application/
    Common/IAppDbContext, ITokenService, IGoogleTokenVerifier, ICurrentUser, ICronJob(+[CronSchedule]), IFeedFetcher, IArticleSummarizer, IGitHubClient
    Shared/Result.cs           # Result<T>, Error(ErrorType, Code, Message) — thuần, KHÔNG tham chiếu IResult (ASP.NET Core)
    Shared/Cqrs.cs             # ICommand<T>/IQuery<T>, CommandHandler<,>/QueryHandler<,> (HandleAsync public abstract, Ok/Fail)
    Features/Auth/             # GoogleLogin, RefreshToken, Logout; Models/AuthModel.cs; AuthIssuer.cs
    Features/News/
      Commands/FetchSourceCommand (tải 1 nguồn), FetchAllFeedsCommand (tải mọi nguồn Active, nhịp của cron)
      Queries/ListArticlesQuery (lọc category/topic/important, tìm trong tiêu đề, sort new|top, phân trang), ListSourcesQuery
      Models/NewsModels.cs, NewsRules.cs (chuẩn hoá link, SHA-256 UrlHash, giới hạn, Classify = chấm độ quan trọng bằng từ khoá)
    Features/Digest/
      Commands/BuildDailyDigestCommand (chọn ~50 bài/ngày), SummarizePendingCommand (AI tóm tắt dần)
      Queries/GetDigestQuery, ListDigestDatesQuery; Models/DigestModel + DigestItemModel; DigestRules.cs (50 bài, hạn ngạch mục, trần mỗi nguồn)
    Features/Repos/
      Commands/FetchTrendingReposCommand (chọn ~12 repo/ngày), SummarizePendingReposCommand; Queries/ListReposQuery; Models/RepoModel; RepoRules.cs
    DependencyInjections.cs    # AddApplication(): AddMediatR
  Newsdesk.Infrastructure/
    Data/AppDbContext.cs : IAppDbContext, SourceSeeder.cs (nạp 16 nguồn RSS lần đầu, chỉ thêm nguồn chưa có)
    Migrations/                # migration nằm ngay đây (hiện có InitNews)
    Auth/JwtOptions, JwtTokenService, GoogleOptions, GoogleTokenVerifier
    News/RssFeedFetcher.cs     # IFeedFetcher: HTTP + XML (RSS 2.0 và Atom)
    Ai/AiOptions, OllamaArticleSummarizer (HttpClient, không package; prompt riêng cho bài và cho repo), DisabledArticleSummarizer
    GitHub/GitHubClient.cs     # IGitHubClient: HttpClient tới api.github.com, không package
    Jobs/CronJobRunner.cs, CronJobRegistry.cs, FetchAllFeedsJob.cs (15 phút), BuildDailyDigestJob (06:00 VN), FetchTrendingReposJob (05:30 VN), SummarizePendingJob (3 phút, cả bài lẫn repo)
    DependencyInjections.cs    # AddInfrastructure(config)
  Newsdesk.API/
    Shared/ResultHttpExtensions.cs   # ToHttpResult — CHỈ ở đây, vì IResult là kiểu ASP.NET Core
    Shared/CurrentUser.cs            # cài ICurrentUser: đọc claim "sub" từ token
    Features/Auth/AuthEndpoints.cs   # /auth/google, /refresh, /logout, /me
    Features/Repos/RepoEndpoints.cs   # GET /repos?limit= (công khai)
    Features/Digest/DigestEndpoints.cs   # GET /digest?date=yyyy-MM-dd, GET /digest/dates (công khai)
    Features/News/NewsEndpoints.cs   # GET /news?category&topic&q&important&sort&page&pageSize, GET /sources (công khai)
    Program.cs
Newsdesk/FE/   # Vite + React, xem mục 6
```

### Quy tắc phụ thuộc (bắt buộc kiểm tra mỗi khi thêm project reference)

```
Domain  ←  Application  ←  Infrastructure
                ↑                 ↑
                └──── API ────────┘
```

- `Domain`: không tham chiếu project nào, không biết EF Core, không biết ASP.NET Core.
- `Application`: chỉ tham chiếu `Domain`. Được dùng `Microsoft.EntityFrameworkCore` (chỉ để có `DbSet<T>` trong `IAppDbContext`), `MediatR`, `Microsoft.Extensions.Logging.Abstractions`. **Không được dùng `Microsoft.AspNetCore.*`** — kể cả `IResult`.
- `Infrastructure`: tham chiếu `Application`, cài đặt các interface của Application (DB, token, đọc feed, cron).
- `API`: tham chiếu `Application`, `Infrastructure`. Là nơi **duy nhất** được biết `IResult`, `HttpContext`, cookie.

### Vertical Slice, Models và Result

- Mỗi tính năng một thư mục dưới `Features/{Entity}/`, tách `Commands/`, `Queries/`, `Models/`. **Models (DTO trả client) nằm trong Application theo từng tính năng, KHÔNG để chung với Domain/Entities** (Domain không được biết hình dạng JSON và không thể tham chiếu ngược Application).
- Handler không `throw` cho lỗi nghiệp vụ — trả `Fail(Error...)` / `Fail(field, message)`. `ToHttpResult()` đổi sang HTTP: 200; `Validation` 400 `ValidationProblem`; `Unauthorized` 401; `NotFound` 404; `Conflict` 409. Thông báo lỗi API dùng tiếng Anh.
- Endpoint chỉ gọi `ISender`, không chứa logic nghiệp vụ.
- Lỗi tải một nguồn tin KHÔNG làm hỏng nhịp: ghi `Source.LastFetchStatus = Failed` + `LastError`, các nguồn khác vẫn chạy.

### Cron (Quartz.NET + bảng CronJobs)

- **Thêm job mới = viết 1 class** implement `ICronJob` + gắn `[CronSchedule("...")]` (Quartz cron 7 trường, có giây, giờ UTC). Class job nằm ở `Infrastructure/Jobs/`. `AddCronJobs(assembly)` quét và đăng ký.
- Quartz chỉ biết một job là `CronJobRunner`, chạy mỗi phút. Bảng `CronJobs` (một dòng/job): `Name`, `CronExpression`, `Status`, `LastRunAt`, `LastDurationMs`, `LastRunStatus`, `LastError`. Đổi lịch / bật-tắt job = sửa dòng trong DB. Dòng đã có không bị ghi đè khi khởi động.
- Quartz dùng job store RAM: **chạy nhiều bản API thì job bị chạy đôi** (shortcut; khi lên Docker thì tách Worker riêng hoặc thêm cờ `Jobs:Enabled`).

### Tin tức

- `Source` (nguồn RSS, danh sách trắng): `Status` Active/Inactive, `LastFetchedAt`, `LastFetchStatus`, `LastError`; thêm/tắt nguồn bằng sửa DB. `SourceSeeder` chỉ thêm nguồn chưa có (theo `FeedUrl`), không ghi đè.
- `Article`: `UrlHash` (SHA-256 của link đã chuẩn hoá) **unique** = chốt chặn chống trùng; `Category` sao chép từ nguồn; ngày tương lai bị kéo về hiện tại.
- Chuỗi hằng số (không enum/bool): `NewsCategory` (Tech/Programming/Politics/Economy), `SourceStatus`, `FetchStatus` ở `Domain/Entities/Source.cs`.
- **Xếp hạng quan trọng (ưu tiên của người đọc: AI, lập trình, tin ảnh hưởng nghề dev)**: `NewsRules.Classify(category, title, summary)` chấm `Article.Priority` 0-100 lúc nạp tin bằng regex từ khoá tiếng Anh: điểm nền theo mục (Programming 30, Tech 25, Economy 10, Politics 5) + AI 25 + Dev 15 + DevImpact 30 (DevImpact cần từ khoá tác động như layoff/licence/CVE/outage VÀ bối cảnh công nghệ). `Article.Topics` lưu dạng `,Ai,DevImpact,` (bọc dấu phẩy để lọc khớp chính xác). `important=true` = Priority ≥ 50; `sort=top` = quan trọng nhất trong 72 giờ gần đây. **Đây là luật từ khoá, bắt sót/nhầm là có (shortcut); FE luôn ghi "chưa phải AI đánh giá". Giai đoạn AI sẽ thay bằng điểm do AI chấm.** Đổi luật phải nạp lại điểm cho bài cũ (chưa có job backfill).
- Shortcut đã biết: `FetchAllFeedsCommand` tải tuần tự trong tiến trình; hai lần tải song song cùng một nguồn sẽ đụng unique `UrlHash` (khi tách sang RabbitMQ at-least-once phải bắt `DbUpdateException` và coi là "đã có").

### AI tóm tắt và bản tin hằng ngày

- **Luồng**: `BuildDailyDigestJob` mỗi ngày 23:00 UTC (= 06:00 giờ Việt Nam) chọn ~50 bài mới và quan trọng nhất của 24 giờ qua → lưu `DigestEntry` (ngày, hạng, bài; giữ lịch sử để xem lại các ngày). `SummarizePendingJob` mỗi 3 phút tóm tắt dần các bài chưa có tóm tắt (tối đa 12 bài hoặc 150 giây mỗi nhịp để không giữ bộ chạy cron) → lưu `ArticleSummary` (Tldr, KeyPoints JSON, WhyItMatters, Language, Model, PromptVersion, Attempts). Người đọc chỉ đọc bản đã lưu qua `GET /digest`. Khởi động lần đầu chưa có bản tin nào thì `SummarizePendingJob` tạo luôn (khỏi chờ 06:00).
- **Chọn bài** (`DigestRules`): bài có đoạn trích ≥ 40 ký tự (chỉ có tiêu đề thì model sẽ bịa nên bỏ), chưa nằm trong bản tin nào, điểm `Priority` cao trước; hạn ngạch mục Programming 16 / Tech 16 / Economy 10 / Politics 8 (thiếu bài thì mục khác lấp chỗ), tối đa 8 bài mỗi nguồn. Ngày bản tin tính theo giờ Việt Nam cố định UTC+7 (shortcut).
- **Đầu vào cho model chỉ là tiêu đề + đoạn trích RSS, KHÔNG phải toàn văn** (bản quyền), nên tóm tắt chỉ viết lại thông tin sẵn có, không thêm chi tiết. Chi tiết thì người đọc bấm link về bài gốc. Nội dung bài là dữ liệu không tin cậy: prompt bọc trong `<article>`, model không có công cụ nào, đầu ra ép theo JSON schema và bị kiểm (rỗng/hỏng thì tính là lần thất bại), FE hiển thị dạng chữ thường + nhãn "Tóm tắt bởi AI, có thể sai".
- **Lỗi**: model trả hỏng → `Attempts++`, thử lại ở nhịp sau, tối đa 3 lần rồi bỏ (bài vẫn hiện trong bản tin, ghi "AI đang tóm tắt"). Ollama chưa chạy/chưa có model → `SummarizerUnavailableException`: dừng cả lô, KHÔNG tính lần thử, tự tóm tắt khi Ollama bật lên.
- **Cấu hình** (mục `Ai`, không có bí mật): `Ai:Provider` = `Ollama` (mặc định) | `None` (tắt AI, bản tin vẫn tạo); `Ai:Language` = `en` (mặc định, model nhỏ viết tiếng Anh tốt hơn) | `vi`; `Ai:Ollama:BaseUrl` (mặc định `http://localhost:11434`), `Ai:Ollama:Model` (mặc định `llama3.2:3b`), `Ai:Ollama:TimeoutSeconds`. Sai tên provider/ngôn ngữ thì báo lỗi lúc khởi động. Thêm provider Claude cần package NuGet của Anthropic (hỏi người dùng trước).
- **Máy dev hiện tại**: GTX 1650 Max-Q chỉ **4 GB VRAM** → dùng model cỡ ~3B (llama3.2:3b, qwen2.5:3b...); 7-8B sẽ tràn VRAM và rất chậm. Ollama phải cài riêng (`ollama pull <model>`); chưa kiểm chất lượng tóm tắt thật trên model thật (mới thử đường đi bằng máy chủ Ollama giả).
- Shortcut: tóm tắt tuần tự từng bài; khi có RabbitMQ sẽ chuyển thành queue `article.summarize` với retry và dead-letter.

### GitHub đang lên

- **Mục tiêu**: giới thiệu ~12 repo mới tạo trong 14 ngày gần đây mà đã ≥ 100 sao ("đang lên"), repo liên quan AI xếp trước (`RepoRules.KindOf`, luật từ khoá), kèm AI tóm tắt "repo làm gì / dành cho ai" và link sang GitHub. Trang FE `/github`, API `GET /repos`.
- **Số request tối thiểu (miễn phí)**: `FetchTrendingReposJob` mỗi ngày 22:30 UTC (= 05:30 VN) gọi MỘT request `search/repositories` (`created:>14 ngày stars:>100 sort=stars per_page=100`) rồi một request README cho mỗi repo được chọn (~12). Chỉ lấy trường cần: tên, mô tả, ngôn ngữ, giấy phép (SPDX), chủ đề, số sao, ngày tạo, README. Loại fork, repo lưu trữ/vô hiệu, danh sách `awesome-*`, repo đã giới thiệu, repo không có README hoặc README đã làm sạch < 200 ký tự (không có gì để tóm tắt trung thực).
- **Token GitHub**: `GitHub:Token` là **tuỳ chọn nhưng nên có**, đặt bằng user-secrets (`dotnet user-secrets set "GitHub:Token" "<token>" --project Newsdesk.API`). Token chỉ-đọc, KHÔNG cần quyền nào (fine-grained token, public repositories). Không token thì hạn mức tìm kiếm tính theo địa chỉ IP (10 request/phút dùng chung với mọi người cùng IP) và thực tế đã bị hết khi thử trên mạng của máy dev; có token là hạn mức riêng. Hết hạn mức/token sai thì job ghi lỗi rõ ràng vào bảng CronJobs, không đoán.
- **README**: chỉ giữ tạm 2000 ký tự đã làm sạch (bỏ ảnh, huy hiệu, thẻ HTML) trong `Repo.ReadmeExcerpt` để làm đầu vào cho AI và **xoá ngay sau khi tóm tắt thành công**. README là dữ liệu không tin cậy: prompt bọc trong `<repo>`, model không có công cụ, đầu ra ép JSON schema, FE hiển thị dạng chữ thường + nhãn AI.
- Shortcut: số sao lưu lúc phát hiện, không cập nhật; repo cũ bỗng nổi (không phải repo mới tạo) không được bắt; chưa có snapshot sao theo ngày để tính "+N sao/ngày".

### Auth: đăng nhập Google + access token + refresh token

- Chỉ có đăng nhập Google. FE lấy Google ID token, `POST /auth/google` → BE kiểm bằng `Google.Apis.Auth` (`Google:ClientId`), tìm/tạo `User` theo `ExternalId` (= `sub`), trả `AuthModel`.
- **Access token** = JWT HS256, sống 15 phút. **Refresh token** = 32 byte ngẫu nhiên, sống 7 ngày, chỉ lưu SHA-256 trong `RefreshTokens`, gửi qua cookie `refresh_token` (HttpOnly, Secure, SameSite=None, Path=/auth).
- `POST /auth/refresh` xoay token; dùng lại token đã thu hồi → thu hồi cả "họ" token (`FamilyId`). `POST /auth/logout` luôn 204.
- `JwtBearer` cấu hình `MapInboundClaims = false` (giữ claim `sub`). CORS policy `"Fe"` cho `http://localhost:3000` có `AllowCredentials()`. `UseHttpsRedirection` chỉ bật ngoài Development.
- `Jwt:Key` (≥ 32 ký tự) và `Google:ClientId` **không đặt trong appsettings**, dùng user-secrets: `dotnet user-secrets set "Jwt:Key" "<chuỗi ngẫu nhiên>" --project Newsdesk.API`. `SameSite=None` là lối tắt cho dev; khi FE và API cùng site (Nginx) thì đổi sang Lax/Strict.

## 4. Database

- SQL Server cài trực tiếp trên máy (không dùng Docker cho SQL Server). Connection string key `DefaultConnection` ở `Newsdesk.API/appsettings.Development.json` (file này nằm trong `.gitignore` vì chứa mật khẩu; mẫu không mật khẩu là `appsettings.Development.json.example`, copy rồi điền, hoặc dùng user-secrets), database tên `Newsdesk` (DB cũ `Streak` còn sót trên máy, xoá tay nếu muốn).
- Entity khai báo độ dài bằng `[MaxLength]` (Id là Guid dạng chuỗi 36 ký tự); index/khoá ngoại cấu hình trong `AppDbContext.OnModelCreating`.
- **Loại / trạng thái lưu dạng CHUỖI, khai báo bằng `static class` hằng số, KHÔNG dùng enum và KHÔNG dùng bool.** Cột "loại" tên `Type`, cột trạng thái tên `Status`. Bật/tắt là `Status = Active/Inactive`, không có cờ `IsActive`.
- Mọi entity kế thừa `BaseEntity` (chỉ giữ Id); mốc tạo/sửa/xoá ở `IAuditable`/`ISoftDelete`, entity nào cần mới implement.
- Không dùng lazy loading, `.Include()` rõ ràng khi cần.

### Migration (Claude chỉ tạo file khi được yêu cầu, KHÔNG tự chạy `database update`)

Development tự `Migrate()` + `SourceSeeder` khi chạy API. Tạo migration:

```bash
cd Newsdesk/BE/Newsdesk
dotnet ef migrations add <Tên> --project Newsdesk.Infrastructure --startup-project Newsdesk.API
dotnet build Newsdesk.slnx   # BUILD LẠI sau khi tạo migration, nếu không dotnet run --no-build sẽ không thấy migration mới
```

Đọc migration sinh ra trước khi báo hoàn thành; báo ngay nếu thấy `DropTable`/`DropColumn`/đổi kiểu gây mất dữ liệu.

## 5. Lệnh thường dùng

```bash
cd Newsdesk/BE/Newsdesk
dotnet build Newsdesk.slnx
dotnet run --project Newsdesk.API --launch-profile https   # https://localhost:7022, tự mở /scalar
```

Đóng build server nếu cần đổi tên thư mục: `dotnet build-server shutdown` (nó giữ khoá thư mục trên Windows).

## 6. Frontend

**React thuần (Vite + React Router)**, TypeScript, Tailwind v4, thư mục `Newsdesk/FE/`. Hướng thiết kế đã chọn: **"Báo in hiện đại"** (xem `Newsdesk/FE/DESIGN.md` và `PRODUCT.md` ở thư mục gốc). Khi làm/sửa UI **phải dùng các skill/plugin thiết kế** (impeccable, ui-ux-pro-max, emil-design-eng...), không tự viết CSS tuỳ ý; hook của impeccable tự quét file sau mỗi lần sửa.

- Cấu trúc kiểu bulletproof-react: `src/app/` (`app.tsx`, `provider.tsx`, `router.tsx`, `routes/home.tsx`, `routes/section.tsx`), `src/features/news/` (`api/`, `hooks/use-articles.ts`, `components/`, `lib/sections.ts` + `format.ts`, `types.ts`), `src/features/auth/` (đăng nhập Google, `authedRequest`), `src/components/layout/` (`site-header` = `Masthead` + `SectionNav`, `site-footer`, `bottom-nav`), `src/components/ui/` (shadcn, base Radix), `src/lib/api-client.ts`, `src/config/env.ts` (đọc biến môi trường, không đọc `import.meta.env` ở nơi khác). Alias `@/*` → `src/*`.
- Route: `/github` (repo GitHub đang lên + tóm tắt AI, mục "GitHub" trên menu), `/ban-tin` và `/ban-tin/:date` (bản tin hằng ngày: ~50 tin, ý chính do AI tóm tắt + link bài gốc, nút ngày trước/sau; trang nhất cũng dùng tóm tắt AI cho bài đầu trang và Điểm nhanh khi có), `/` (trang nhất: bài đầu trang = quan trọng nhất, "Điểm nhanh", dải "Ảnh hưởng nghề dev", các khối mục), `/muc/:slug` (slug: `ai`, `lap-trinh`, `nghe-dev`, `cong-nghe`, `kinh-te`, `chinh-tri`, `tat-ca` cho tìm kiếm; bộ lọc `q`, `sort`, `important` nằm trên URL). `/muc/ai` và `/muc/nghe-dev` là chủ đề (`topic`), các mục còn lại là `category`.
- Token màu ở `src/index.css` (CSS variable → `@theme inline`), đừng viết mã hex trong component. Font: Literata (tiêu đề tin, class `.headline`) + Be Vietnam Pro (giao diện). Sáng mặc định, tối qua `theme-provider.tsx` (key `newsdesk-theme`), không dùng `next-themes`.
- Đăng nhập là popup `Dialog` shadcn (`features/auth/components/auth-modal.tsx`); đọc tin KHÔNG cần đăng nhập. Cần `VITE_GOOGLE_CLIENT_ID` trong `.env.local`.
- `request()` chỉ gửi `Content-Type` khi có body (GET khỏi phải preflight CORS). Base URL API từ `VITE_API_URL` (mặc định `https://localhost:7022`, `.env.local`).
- Vite chạy cổng 3000 khớp CORS policy `"Fe"` ở BE (đụng cổng FE thì sửa cả 2 chỗ). Nếu cổng 3000 đã bận, Vite tự nhảy cổng khác và mọi lệnh gọi API sẽ bị CORS chặn.
- Lệnh: `cd Newsdesk/FE && npm run dev` (cổng 3000), `npm run build`, `npm run lint`.
- Chưa làm: trang Khoá học (không có link chết trên menu), ảnh bài dùng thẳng URL của feed (ảnh lỗi thì ẩn).

## 7. Việc còn thiếu / TODO

- Các giai đoạn còn lại ở mục 1; chưa có `User.Role`/endpoint quản trị nguồn; chưa có job nạp lại `Priority`/`Topics` cho bài cũ khi đổi luật.
- Chưa có test project (cả BE và FE). Chưa có unique index cho `Users.ExternalId`; chưa tự điền `CreatedBy/UpdatedAt...`; chưa có query filter soft-delete.
- Chưa có rate limiting cho endpoint công khai (`/news`, `/sources`), chưa có trang Điều khoản/Quyền riêng tư.
- `appsettings.Development.json` chứa connection string `sa` + mật khẩu (chỉ cho máy dev) — không commit lên repo công khai; nên chuyển sang user-secrets.
- Repo git, remote `https://github.com/DuyNM0601/Streak.git` (người dùng tự đổi tên repo rồi `git remote set-url`), branch `main`. `.vscode/mcp.json` còn trỏ đường dẫn thư mục gốc cũ `Desktop\Streak`: sửa khi đổi tên thư mục gốc.

## 8. Cách làm việc với Claude

- Thay đổi kiến trúc lớn: nói rõ kế hoạch, chờ xác nhận trước khi sửa nhiều file.
- Sau khi sửa code, luôn `dotnet build` để xác nhận; nếu có thể, chạy thử thật (dùng database tạm, không đụng DB của người dùng, xoá sau khi thử) để bắt lỗi runtime.
- Không tự chạy `dotnet ef database update` hoặc tạo migration khi chưa được yêu cầu rõ.
- Không thêm NuGet package mới khi chưa hỏi, trừ khi để sửa lỗi build.
- Không commit secret/connection string có mật khẩu thật.
- Không tự `git commit` hoặc `git push` — người dùng tự làm. Chỉ đưa tên nhánh/commit message và lệnh git khi người dùng **hỏi**.
