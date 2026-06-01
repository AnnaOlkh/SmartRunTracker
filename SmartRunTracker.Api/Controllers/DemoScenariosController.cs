using Microsoft.AspNetCore.Mvc;
using SmartRunTracker.Application.Common;

namespace SmartRunTracker.Api.Controllers;

[ApiController]
[Route("api/demo-scenarios")]
public sealed class DemoScenariosController : ControllerBase
{
    private readonly IDemoScenarioService _demoScenarioService;

    public DemoScenariosController(IDemoScenarioService demoScenarioService)
    {
        _demoScenarioService = demoScenarioService;
    }

    [HttpPost("maintain")]
    public async Task<ActionResult<DemoScenarioResultDto>> Maintain(
        CancellationToken cancellationToken)
    {
        var result = await _demoScenarioService.SeedMaintainScenarioAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("progress")]
    public async Task<ActionResult<DemoScenarioResultDto>> Progress(
        CancellationToken cancellationToken)
    {
        var result = await _demoScenarioService.SeedProgressScenarioAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("deload")]
    public async Task<ActionResult<DemoScenarioResultDto>> Deload(
        CancellationToken cancellationToken)
    {
        var result = await _demoScenarioService.SeedDeloadScenarioAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("skipped-sessions")]
    public async Task<ActionResult<DemoScenarioResultDto>> SkippedSessions(
        CancellationToken cancellationToken)
    {
        var result = await _demoScenarioService.SeedSkippedSessionsScenarioAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("low-consistency")]
    public async Task<ActionResult<DemoScenarioResultDto>> LowConsistency(
        CancellationToken cancellationToken)
    {
        var result = await _demoScenarioService.SeedLowConsistencyScenarioAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("six-days")]
    public async Task<ActionResult<DemoScenarioResultDto>> SixDays(
        CancellationToken cancellationToken)
    {
        var result = await _demoScenarioService.SeedSixDaysScenarioAsync(
            cancellationToken);

        return Ok(result);
    }
}