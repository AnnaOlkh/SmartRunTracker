namespace SmartRunTracker.Application.RunningGoals;

public sealed record CreateRunningGoalRequest(
    decimal TargetDistanceKm,
    int TargetPaceSecondsPerKm,
    DateOnly? GoalDate
);