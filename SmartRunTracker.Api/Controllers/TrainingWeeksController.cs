using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.TrainingPlans;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/training-weeks")]
public sealed class TrainingWeeksController : ControllerBase
{
    private readonly ITrainingPlanService _trainingPlanService;

    public TrainingWeeksController(ITrainingPlanService trainingPlanService)
    {
        _trainingPlanService = trainingPlanService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<TrainingWeekDto>> Generate(
        GenerateTrainingWeekRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var week = await _trainingPlanService.GenerateWeekAsync(
                DemoUser.Id,
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
            DemoUser.Id,
            cancellationToken);

        return Ok(weeks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainingWeekDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var week = await _trainingPlanService.GetWeekByIdAsync(
            DemoUser.Id,
            id,
            cancellationToken);

        if (week is null)
        {
            return NotFound();
        }

        return Ok(week);
    }
}