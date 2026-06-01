namespace SmartRunTracker.Application.Common;

public sealed record DemoScenarioResultDto(
    string Scenario,
    DateOnly WeekStartDate,
    string Message
);