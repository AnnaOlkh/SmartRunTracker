using Microsoft.Extensions.Options;
using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IAuthRepository authRepository,
        ITokenService tokenService,
        IPasswordHashService passwordHashService,
        IOptions<JwtOptions> jwtOptions)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
        _passwordHashService = passwordHashService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new ArgumentException("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ArgumentException("Password must contain at least 6 characters.");
        }

        var emailExists = await _authRepository.EmailExistsAsync(email, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new User
        {
            DisplayName = request.DisplayName.Trim(),
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = _passwordHashService.HashPassword(user, request.Password);

        _authRepository.AddUser(user);

        await _authRepository.SaveChangesAsync(cancellationToken);

        return await CreateAuthResultAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _authRepository.GetUserByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isValidPassword = _passwordHashService.VerifyPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await CreateAuthResultAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);

        var storedToken = await _authRepository.GetRefreshTokenWithUserAsync(
            tokenHash,
            cancellationToken);

        if (storedToken is null || !storedToken.IsActive(now))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = storedToken.User;

        var newRefreshToken = _tokenService.CreateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshToken);

        storedToken.RevokedAt = now;
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;

        _authRepository.AddRefreshToken(new UserRefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenExpirationDays)
        });

        var accessToken = _tokenService.CreateAccessToken(
            user,
            now,
            out var accessTokenExpiresAt);

        await _authRepository.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            user.Id,
            user.DisplayName,
            user.Email,
            accessToken,
            newRefreshToken,
            accessTokenExpiresAt);
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);

        var storedToken = await _authRepository.GetRefreshTokenWithUserAsync(
            tokenHash,
            cancellationToken);

        if (storedToken is null)
        {
            return;
        }

        storedToken.RevokedAt ??= DateTimeOffset.UtcNow;

        await _authRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserResponse?> GetMeAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new CurrentUserResponse(
            user.Id,
            user.DisplayName,
            user.Email);
    }

    private async Task<AuthResult> CreateAuthResultAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var accessToken = _tokenService.CreateAccessToken(
            user,
            now,
            out var accessTokenExpiresAt);

        var refreshToken = _tokenService.CreateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);

        _authRepository.AddRefreshToken(new UserRefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenExpirationDays)
        });

        await _authRepository.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            user.Id,
            user.DisplayName,
            user.Email,
            accessToken,
            refreshToken,
            accessTokenExpiresAt);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}