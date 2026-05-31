using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts;

public sealed record WorkoutDto(
    int Id,
    int UserId,
    int? PlannedSessionId,
    DateTimeOffset StartedAt,
    decimal DistanceKm,
    int DurationSeconds,
    int AveragePaceSecondsPerKm,
    int Rpe,
    int SessionLoad,
    WorkoutType Type,
    WorkoutSource Source,
    string? Notes,
    DateTimeOffset CreatedAt
);