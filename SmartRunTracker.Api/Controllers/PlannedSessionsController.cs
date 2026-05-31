using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.TrainingPlans;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/planned-sessions")]
public sealed class PlannedSessionsController : ControllerBase
{
    private readonly ITrainingPlanService _trainingPlanService;

    public PlannedSessionsController(ITrainingPlanService trainingPlanService)
    {
        _trainingPlanService = trainingPlanService;
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
                DemoUser.Id,
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
                DemoUser.Id,
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