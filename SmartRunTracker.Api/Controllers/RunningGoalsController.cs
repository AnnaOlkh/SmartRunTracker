using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.RunningGoals;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/running-goals")]
public sealed class RunningGoalsController : ControllerBase
{
    private readonly IRunningGoalService _runningGoalService;

    public RunningGoalsController(IRunningGoalService runningGoalService)
    {
        _runningGoalService = runningGoalService;
    }

    [HttpGet("active")]
    public async Task<ActionResult<RunningGoalDto>> GetActive(
        CancellationToken cancellationToken)
    {
        var goal = await _runningGoalService.GetActiveAsync(
            DemoUser.Id,
            cancellationToken);

        if (goal is null)
        {
            return NotFound(new
            {
                message = "Active running goal was not found."
            });
        }

        return Ok(goal);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RunningGoalDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var goals = await _runningGoalService.GetAllAsync(
            DemoUser.Id,
            cancellationToken);

        return Ok(goals);
    }

    [HttpPost]
    public async Task<ActionResult<RunningGoalDto>> Create(
        CreateRunningGoalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var goal = await _runningGoalService.CreateAsync(
                DemoUser.Id,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetActive),
                new { },
                goal);
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