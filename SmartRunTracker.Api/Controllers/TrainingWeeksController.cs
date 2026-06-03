using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.TrainingPlans;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/training-weeks")]
public sealed class TrainingWeeksController : ControllerBase
{
    private readonly ITrainingPlanService _trainingPlanService;
    private readonly ICurrentUserService _currentUserService;

    public TrainingWeeksController(ITrainingPlanService trainingPlanService, ICurrentUserService currentUserService)
    {
        _trainingPlanService = trainingPlanService;
        _currentUserService = currentUserService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<TrainingWeekDto>> Generate(
        GenerateTrainingWeekRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var week = await _trainingPlanService.GenerateWeekAsync(
                _currentUserService.UserId,
                request,
                cancellationToken);

            return Ok(week);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrainingWeekDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var weeks = await _trainingPlanService.GetWeeksAsync(
            _currentUserService.UserId,
            cancellationToken);

        return Ok(weeks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainingWeekDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var week = await _trainingPlanService.GetWeekByIdAsync(
            _currentUserService.UserId,
            id,
            cancellationToken);

        if (week is null)
        {
            return NotFound();
        }

        return Ok(week);
    }
}