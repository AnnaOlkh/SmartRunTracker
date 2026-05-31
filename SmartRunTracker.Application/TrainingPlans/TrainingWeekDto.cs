using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.TrainingPlans;

public sealed record TrainingWeekDto(
    int Id,
    int UserId,
    int RunningGoalId,
    DateOnly WeekStartDate,
    int TargetWorkoutCount,
    AdjustmentMode AdjustmentMode,
    int TargetWeekDurationSeconds,
    TrainingWeekStatus Status,
    string? Explanation,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<PlannedSessionDto> PlannedSessions
);