using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.TrainingPlans;

public sealed record GeneratedTrainingWeek(
    AdjustmentMode AdjustmentMode,
    int TargetWorkoutCount,
    int TargetWeekDurationSeconds,
    string Explanation,
    IReadOnlyList<GeneratedPlannedSession> Sessions
);

public sealed record GeneratedPlannedSession(
    WorkoutType Type,
    WorkoutIntensity Intensity,
    int TargetDurationSeconds,
    decimal? TargetDistanceKm,
    int? TargetPaceSecondsPerKm,
    string? Notes,
    string Reason,
    int SortOrder
);