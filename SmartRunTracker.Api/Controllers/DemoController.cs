using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Api.Auth;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.Demo;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    private readonly IDemoUserService _demoUserService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDemoSeedService _demoSeedService;

    public DemoController(IDemoUserService demoUserService, ICurrentUserService currentUserService,
        IDemoSeedService demoSeedService)
    {
        _demoUserService = demoUserService;
        _currentUserService = currentUserService;
        _demoSeedService = demoSeedService;
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
    [HttpPost("seed-may-2026")]
    public async Task<ActionResult<DemoSeedResult>> SeedMay2026(
        CancellationToken cancellationToken)
    {
        var result = await _demoSeedService.SeedMay2026Async(
            _currentUserService.UserId,
            cancellationToken);

        return Ok(result);
    }
}