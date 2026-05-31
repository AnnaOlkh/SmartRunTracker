namespace SmartRunTracker.Application.RunningGoals;

public sealed record RunningGoalDto(
    int Id,
    int UserId,
    decimal TargetDistanceKm,
    int TargetPaceSecondsPerKm,
    int TargetFinishSeconds,
    DateOnly? GoalDate,
    bool IsActive,
    DateTimeOffset CreatedAt
);