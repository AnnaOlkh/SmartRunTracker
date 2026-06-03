namespace SmartRunTracker.Application.Workouts;

public sealed record WorkoutSplitDto(
    int Id,
    int SplitNumber,
    decimal DistanceKm,
    int DurationSeconds,
    int AveragePaceSecondsPerKm,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt);