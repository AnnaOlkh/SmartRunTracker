namespace SmartRunTracker.Application.Auth;

public sealed record RegisterRequest(
    string DisplayName,
    string Email,
    string Password);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record AuthResponse(
    int UserId,
    string DisplayName,
    string Email,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt);

public sealed record AuthResult(
    int UserId,
    string DisplayName,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);

public sealed record CurrentUserResponse(
    int UserId,
    string DisplayName,
    string Email);