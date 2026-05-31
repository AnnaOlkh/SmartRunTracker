using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    private readonly IDemoUserService _demoUserService;

    public DemoController(IDemoUserService demoUserService)
    {
        _demoUserService = demoUserService;
    }

    [HttpPost("bootstrap")]
    public async Task<IActionResult> Bootstrap(
        CancellationToken cancellationToken)
    {
        await _demoUserService.EnsureDemoUserAsync(cancellationToken);

        return Ok(new
        {
            userId = DemoUser.Id,
            displayName = DemoUser.DisplayName,
            message = "Demo user is ready."
        });
    }
}