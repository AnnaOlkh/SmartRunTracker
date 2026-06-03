namespace SmartRunTracker.Application.Workouts;

public sealed record WorkoutRoutePointDto(
    int Id,
    int Order,
    decimal Latitude,
    decimal Longitude,
    decimal? ElevationMeters,
    DateTimeOffset? RecordedAt,
    decimal DistanceFromStartMeters,
    int? SecondsFromStart,
    int? PaceSecondsPerKm);