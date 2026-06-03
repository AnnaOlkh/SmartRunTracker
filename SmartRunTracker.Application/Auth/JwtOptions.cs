namespace SmartRunTracker.Application.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 20;

    public int RefreshTokenExpirationDays { get; set; } = 14;
}