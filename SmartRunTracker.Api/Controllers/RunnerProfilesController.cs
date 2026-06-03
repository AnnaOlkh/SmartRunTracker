using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.RunnerProfiles;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/runner-profile")]
public sealed class RunnerProfilesController : ControllerBase
{
    private readonly IRunnerProfileService _runnerProfileService;
    private readonly ICurrentUserService _currentUserService;

    public RunnerProfilesController(IRunnerProfileService runnerProfileService, ICurrentUserService currentUserService)
    {
        _runnerProfileService = runnerProfileService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<RunnerProfileDto>> Get(
        CancellationToken cancellationToken)
    {
        var profile = await _runnerProfileService.GetOrCreateAsync(
            _currentUserService.UserId,
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
                _currentUserService.UserId,
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