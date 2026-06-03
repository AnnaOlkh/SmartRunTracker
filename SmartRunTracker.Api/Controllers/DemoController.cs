using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Api.Auth;
using SmartRunTracker.Application.Common;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    private readonly IDemoUserService _demoUserService;
    private readonly ICurrentUserService _currentUserService;

    public DemoController(IDemoUserService demoUserService, ICurrentUserService currentUserService)
    {
        _demoUserService = demoUserService;
        _currentUserService = currentUserService;
    }

    [HttpPost("bootstrap")]
    public async Task<IActionResult> Bootstrap(
        CancellationToken cancellationToken)
    {
        await _demoUserService.EnsureDemoUserAsync(cancellationToken);

        return Ok(new
        {
            userId = _currentUserService.UserId,
            displayName = DemoUser.DisplayName,
            message = "Demo user is ready."
        });
    }
}