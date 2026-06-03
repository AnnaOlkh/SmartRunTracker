using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.Auth;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<User?> GetUserByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserRefreshToken?> GetRefreshTokenWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    void AddUser(User user);

    void AddRefreshToken(UserRefreshToken refreshToken);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}