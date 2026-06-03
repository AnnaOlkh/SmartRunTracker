using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.TrainingPlans;
using SmartRunTracker.Application.Workouts;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workouts")]
public sealed class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;
    private readonly ICurrentUserService _currentUserService;

    public WorkoutsController(IWorkoutService workoutService, ICurrentUserService currentUserService)
    {
        _workoutService = workoutService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkoutDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var workouts = await _workoutService.GetAllAsync(
            _currentUserService.UserId,
            cancellationToken);

        return Ok(workouts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkoutDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var workout = await _workoutService.GetByIdAsync(
            _currentUserService.UserId,
            id,
            cancellationToken);

        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }
    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<WorkoutDetailsDto>> GetDetails(
    int id,
    CancellationToken cancellationToken)
    {
        var workout = await _workoutService.GetDetailsAsync(
            _currentUserService.UserId,
            id,
            cancellationToken);

        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutDto>> Create(
        CreateWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workout = await _workoutService.CreateAsync(
                _currentUserService.UserId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = workout.Id },
                workout);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkoutDto>> Update(
        int id,
        UpdateWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workout = await _workoutService.UpdateAsync(
                _currentUserService.UserId,
                id,
                request,
                cancellationToken);

            if (workout is null)
            {
                return NotFound();
            }

            return Ok(workout);
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
        var deleted = await _workoutService.DeleteAsync(
            _currentUserService.UserId,
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
    [HttpGet("{id:int}/available-planned-sessions")]
    public async Task<ActionResult<IReadOnlyList<PlannedSessionDto>>> GetAvailablePlannedSessions(
    int id,
    CancellationToken cancellationToken)
    {
        var sessions = await _workoutService.GetAvailablePlannedSessionsAsync(
            _currentUserService.UserId,
            id,
            cancellationToken);

        return Ok(sessions);
    }

    [HttpPatch("{id:int}/planned-session")]
    public async Task<ActionResult<WorkoutDetailsDto>> LinkPlannedSession(
        int id,
        LinkWorkoutPlannedSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workout = await _workoutService.LinkPlannedSessionAsync(
                _currentUserService.UserId,
                id,
                request,
                cancellationToken);

            if (workout is null)
            {
                return NotFound();
            }

            return Ok(workout);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}