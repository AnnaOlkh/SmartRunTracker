using SmartRunTracker.Application.Workouts.Analysis;

namespace SmartRunTracker.Application.Workouts;

public sealed record WorkoutDetailsDto(
    WorkoutDto Summary,
    WorkoutPlannedSessionDto? PlannedSession,
    PlannedVsActualDto? PlannedVsActual,
    IReadOnlyList<WorkoutInsightDto> Insights,
    IReadOnlyList<WorkoutRoutePointDto> RoutePoints,
    IReadOnlyList<WorkoutSplitDto> Splits)
{
    public bool HasRouteData => RoutePoints.Count > 0;

    public bool HasSplits => Splits.Count > 0;
}