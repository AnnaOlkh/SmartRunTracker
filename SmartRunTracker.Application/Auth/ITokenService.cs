using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.Auth;

public interface ITokenService
{
    string CreateAccessToken(User user, DateTimeOffset now, out DateTimeOffset expiresAt);

    string CreateRefreshToken();

    string HashRefreshToken(string refreshToken);
}