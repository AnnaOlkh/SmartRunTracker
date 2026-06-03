namespace SmartRunTracker.Application.ExternalWorkouts.Models;

public class ExternalWorkoutPoint
{
    public int Order { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public decimal? ElevationMeters { get; set; }

    public DateTimeOffset? RecordedAt { get; set; }
}