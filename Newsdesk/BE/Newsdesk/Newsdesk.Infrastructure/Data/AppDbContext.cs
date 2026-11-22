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
        }
    }
}
