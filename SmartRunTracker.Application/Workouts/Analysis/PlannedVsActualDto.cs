namespace SmartRunTracker.Application.Workouts.Analysis;

public sealed record PlannedVsActualDto(
    PlannedVsActualStatus Status,
    IReadOnlyList<string> Messages,
    PlannedVsActualMetricDto Duration,
    PlannedVsActualMetricDto? Distance,
    PlannedVsActualMetricDto? Pace,
    PlannedVsActualTypeDto Type,
    PlannedVsActualIntensityDto Intensity);