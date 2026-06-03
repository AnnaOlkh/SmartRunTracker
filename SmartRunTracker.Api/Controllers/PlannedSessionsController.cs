using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.TrainingPlans;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/planned-sessions")]
public sealed class PlannedSessionsController : ControllerBase
{
    private readonly ITrainingPlanService _trainingPlanService;
    private readonly ICurrentUserService _currentUserService;

    public PlannedSessionsController(ITrainingPlanService trainingPlanService, ICurrentUserService currentUserService)
    {
        _trainingPlanService = trainingPlanService;
        _currentUserService = currentUserService;
    }

    [HttpPatch("{id:int}/schedule")]
    public async Task<ActionResult<PlannedSessionDto>> Schedule(
        int id,
        SchedulePlannedSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _trainingPlanService.ScheduleSessionAsync(
                _currentUserService.UserId,
                id,
                request,
                cancellationToken);

            if (session is null)
            {
                return NotFound();
            }

            return Ok(session);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPatch("{id:int}/skip")]
    public async Task<ActionResult<PlannedSessionDto>> Skip(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _trainingPlanService.SkipSessionAsync(
                _currentUserService.UserId,
                id,
                cancellationToken);

            if (session is null)
            {
                return NotFound();
            }

            return Ok(session);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}