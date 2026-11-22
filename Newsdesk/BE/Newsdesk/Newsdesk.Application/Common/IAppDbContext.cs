using Microsoft.EntityFrameworkCore;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Common
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }

        DbSet<RefreshToken> RefreshTokens { get; }

        DbSet<Source> Sources { get; }

        DbSet<Article> Articles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
