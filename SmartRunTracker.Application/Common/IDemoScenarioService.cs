namespace SmartRunTracker.Application.Common;

public interface IDemoScenarioService
{
    Task<DemoScenarioResultDto> SeedMaintainScenarioAsync(
        CancellationToken cancellationToken = default);

    Task<DemoScenarioResultDto> SeedProgressScenarioAsync(
        CancellationToken cancellationToken = default);

    Task<DemoScenarioResultDto> SeedDeloadScenarioAsync(
        CancellationToken cancellationToken = default);

    Task<DemoScenarioResultDto> SeedSkippedSessionsScenarioAsync(
        CancellationToken cancellationToken = default);

    Task<DemoScenarioResultDto> SeedLowConsistencyScenarioAsync(
        CancellationToken cancellationToken = default);

    Task<DemoScenarioResultDto> SeedSixDaysScenarioAsync(
        CancellationToken cancellationToken = default);
}