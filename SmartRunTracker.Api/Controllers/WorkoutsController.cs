using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.Workouts;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/workouts")]
public sealed class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;

    public WorkoutsController(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkoutDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var workouts = await _workoutService.GetAllAsync(
            DemoUser.Id,
            cancellationToken);

        return Ok(workouts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkoutDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var workout = await _workoutService.GetByIdAsync(
            DemoUser.Id,
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
                DemoUser.Id,
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
                DemoUser.Id,
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
            DemoUser.Id,
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}