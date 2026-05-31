using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts;

public sealed record UpdateWorkoutRequest(
    DateTimeOffset StartedAt,
    decimal DistanceKm,
    int DurationSeconds,
    int Rpe,
    WorkoutType Type,
    string? Notes
);