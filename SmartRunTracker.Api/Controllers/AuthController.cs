using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Auth;
using SmartRunTracker.Application.Common;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private const string RefreshTokenCookieName = "smartRunTracker.refreshToken";

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
    RegisterRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(ToResponse(result));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
    LoginRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(ToResponse(result));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
    CancellationToken cancellationToken)
    {
        try
        {
            if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
            {
                return Unauthorized(new { message = "Refresh token cookie was not found." });
            }

            var result = await _authService.RefreshAsync(refreshToken, cancellationToken);

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(ToResponse(result));
        }
        catch (UnauthorizedAccessException exception)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(new { message = exception.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
        {
            await _authService.LogoutAsync(refreshToken, cancellationToken);
        }

        DeleteRefreshTokenCookie();

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me(
    CancellationToken cancellationToken)
    {
        var response = await _authService.GetMeAsync(
            _currentUserService.UserId,
            cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }
    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
                Path = "/api/auth"
            });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/api/auth"
            });
    }

    private static AuthResponse ToResponse(AuthResult result)
    {
        return new AuthResponse(
            result.UserId,
            result.DisplayName,
            result.Email,
            result.AccessToken,
            result.AccessTokenExpiresAt);
    }
}