using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts;

public sealed record WorkoutPlannedSessionDto(
    int Id,
    WorkoutType Type,
    WorkoutIntensity Intensity,
    int TargetDurationSeconds,
    decimal? TargetDistanceKm,
    int? TargetPaceSecondsPerKm,
    DateTimeOffset? ScheduledFor,
    PlannedSessionStatus Status);