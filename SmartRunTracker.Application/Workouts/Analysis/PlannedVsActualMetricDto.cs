namespace SmartRunTracker.Application.Workouts.Analysis;

public sealed record PlannedVsActualMetricDto(
    string Name,
    decimal? PlannedValue,
    decimal ActualValue,
    decimal? Difference,
    decimal? DifferencePercent,
    string Unit,
    PlannedVsActualMetricStatus Status,
    string Message);