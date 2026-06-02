namespace SmartRunTracker.Application.ExternalWorkouts.Analysis;

public class AnalyzedRoutePoint
{
    public int Order { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public decimal? ElevationMeters { get; set; }

    public DateTimeOffset? RecordedAt { get; set; }

    public decimal DistanceFromStartMeters { get; set; }

    public int? SecondsFromStart { get; set; }

    public int? PaceSecondsPerKm { get; set; }
}