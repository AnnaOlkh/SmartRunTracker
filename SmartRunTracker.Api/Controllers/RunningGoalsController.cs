using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.RunningGoals;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/running-goals")]
public sealed class RunningGoalsController : ControllerBase
{
    private readonly IRunningGoalService _runningGoalService;
    private readonly ICurrentUserService _currentUserService;

    public RunningGoalsController(IRunningGoalService runningGoalService, ICurrentUserService currentUserService)
    {
        _runningGoalService = runningGoalService;
        _currentUserService = currentUserService;
    }

    [HttpGet("active")]
    public async Task<ActionResult<RunningGoalDto>> GetActive(
        CancellationToken cancellationToken)
    {
        var goal = await _runningGoalService.GetActiveAsync(
            _currentUserService.UserId,
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
            _currentUserService.UserId,
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
                _currentUserService.UserId,
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

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<RunningGoalDto>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var goal = await _runningGoalService.ActivateAsync(
                _currentUserService.UserId,
                id,
                cancellationToken);

            if (goal is null)
            {
                return NotFound();
            }

            return Ok(goal);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _runningGoalService.DeleteAsync(
            _currentUserService.UserId,
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}