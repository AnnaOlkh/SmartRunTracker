using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SmartRunTracker.Application.Common;

namespace SmartRunTracker.Api.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public int UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdValue =
                user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("Authenticated user id was not found.");
            }

            return userId;
        }
    }
}