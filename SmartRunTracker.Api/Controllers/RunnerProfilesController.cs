using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.RunnerProfiles;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/runner-profile")]
public sealed class RunnerProfilesController : ControllerBase
{
    private readonly IRunnerProfileService _runnerProfileService;

    public RunnerProfilesController(IRunnerProfileService runnerProfileService)
    {
        _runnerProfileService = runnerProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<RunnerProfileDto>> Get(
        CancellationToken cancellationToken)
    {
        var profile = await _runnerProfileService.GetOrCreateAsync(
            DemoUser.Id,
            cancellationToken);

        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<RunnerProfileDto>> Update(
        UpdateRunnerProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _runnerProfileService.UpdateAsync(
                DemoUser.Id,
                request,
                cancellationToken);

            return Ok(profile);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}