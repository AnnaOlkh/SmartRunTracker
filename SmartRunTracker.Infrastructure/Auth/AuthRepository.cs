using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.Auth;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Infrastructure.Persistence;

namespace SmartRunTracker.Infrastructure.Auth;

public sealed class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _dbContext;

    public AuthRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public Task<UserRefreshToken?> GetRefreshTokenWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.UserRefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public void AddUser(User user)
    {
        _dbContext.Users.Add(user);
    }

    public void AddRefreshToken(UserRefreshToken refreshToken)
    {
        _dbContext.UserRefreshTokens.Add(refreshToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}