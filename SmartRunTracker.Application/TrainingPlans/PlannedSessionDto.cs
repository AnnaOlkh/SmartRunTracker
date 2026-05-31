using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.TrainingPlans;

public sealed record PlannedSessionDto(
    int Id,
    int TrainingWeekId,
    WorkoutType Type,
    WorkoutIntensity Intensity,
    int TargetDurationSeconds,
    decimal? TargetDistanceKm,
    int? TargetPaceSecondsPerKm,
    DateTimeOffset? ScheduledFor,
    PlannedSessionStatus Status,
    string? Notes,
    string? Reason,
    int SortOrder
);