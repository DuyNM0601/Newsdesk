using Microsoft.EntityFrameworkCore;
using Newsdesk.Application.Common;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Infrastructure.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Source> Sources { get; set; }

        public DbSet<Article> Articles { get; set; }

        public DbSet<ArticleSummary> ArticleSummaries { get; set; }

        public DbSet<DigestEntry> DigestEntries { get; set; }

        public DbSet<Repo> Repos { get; set; }

        // Chỉ Infrastructure (CronJobRunner) dùng, nên không có trong IAppDbContext.
        public DbSet<CronJob> CronJobs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tra token theo TokenHash ở mỗi lần làm mới nên cần unique index.
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.TokenHash).IsUnique();
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.FamilyId);
            modelBuilder.Entity<RefreshToken>().HasOne<User>().WithMany().HasForeignKey(x => x.UserId);

            modelBuilder.Entity<CronJob>().HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<Source>().HasIndex(x => x.FeedUrl).IsUnique();

            // Unique UrlHash: chốt chặn cuối để cùng một bài không bao giờ vào database hai lần.
            modelBuilder.Entity<Article>().HasIndex(x => x.UrlHash).IsUnique();
            // Danh sách tin luôn sắp theo ngày đăng giảm dần, có hoặc không lọc theo chủ đề.
            modelBuilder.Entity<Article>().HasIndex(x => x.PublishedAt).IsDescending();
            modelBuilder.Entity<Article>().HasIndex(x => new { x.Category, x.PublishedAt }).IsDescending(false, true);
            modelBuilder.Entity<Article>().HasOne(x => x.Source).WithMany().HasForeignKey(x => x.SourceId);

            // Mỗi bài tối đa một bản tóm tắt (cập nhật tại chỗ khi thử lại), tra theo ArticleId.
            modelBuilder.Entity<ArticleSummary>().HasIndex(x => x.ArticleId).IsUnique();
            modelBuilder.Entity<ArticleSummary>().HasOne<Article>().WithMany().HasForeignKey(x => x.ArticleId);

            // Một bài chỉ nằm một lần trong bản tin của một ngày; danh sách của ngày luôn đọc theo thứ hạng.
            modelBuilder.Entity<DigestEntry>().HasIndex(x => new { x.DigestDate, x.ArticleId }).IsUnique();
            modelBuilder.Entity<DigestEntry>().HasIndex(x => new { x.DigestDate, x.Rank });
            modelBuilder.Entity<DigestEntry>().HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId);

            // Mỗi repo chỉ được giới thiệu một lần; danh sách luôn đọc theo (ngày giới thiệu mới nhất, hạng).
            modelBuilder.Entity<Repo>().HasIndex(x => x.FullName).IsUnique();
            modelBuilder.Entity<Repo>().HasIndex(x => new { x.DiscoveredOn, x.Rank });
        }
    }
}
