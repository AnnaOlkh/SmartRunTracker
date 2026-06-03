using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Api.Auth;
using SmartRunTracker.Api.Contracts.WorkoutImports;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.ExternalWorkouts.Abstractions;
using SmartRunTracker.Application.ExternalWorkouts.Models;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workout-imports")]
public sealed class WorkoutImportsController : ControllerBase
{
    private readonly IExternalWorkoutImportService _externalWorkoutImportService;
    private readonly ICurrentUserService _currentUserService;

    public WorkoutImportsController(
        IExternalWorkoutImportService externalWorkoutImportService, ICurrentUserService currentUserService)
    {
        _externalWorkoutImportService = externalWorkoutImportService;
        _currentUserService = currentUserService;
    }

    [HttpPost("gpx")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportGpx(
        [FromForm] ImportGpxWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest(new { message = "GPX file is required." });
        }

        if (request.File.Length == 0)
        {
            return BadRequest(new { message = "GPX file is empty." });
        }

        if (!request.File.FileName.EndsWith(".gpx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only .gpx files are supported." });
        }

        var options = new ExternalWorkoutImportOptions
        {
            UserId = _currentUserService.UserId,
            PlannedSessionId = request.PlannedSessionId,
            WorkoutType = request.WorkoutType,
            Rpe = request.Rpe,
            Notes = request.Notes
        };

        await using var stream = request.File.OpenReadStream();

        var result = await _externalWorkoutImportService.ImportGpxAsync(
            stream,
            request.File.FileName,
            options,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Created(
            $"/api/workouts/{result.Workout!.Id}",
            result.Workout);
    }
}