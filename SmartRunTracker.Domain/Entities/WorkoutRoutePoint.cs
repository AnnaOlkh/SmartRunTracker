namespace SmartRunTracker.Domain.Entities;

public class WorkoutRoutePoint
{
    public int Id { get; set; }

    public int WorkoutId { get; set; }

    public int Order { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public decimal? ElevationMeters { get; set; }

    public DateTimeOffset? RecordedAt { get; set; }

    public decimal DistanceFromStartMeters { get; set; }

    public int? SecondsFromStart { get; set; }

    public int? PaceSecondsPerKm { get; set; }

    public Workout Workout { get; set; } = null!;
}