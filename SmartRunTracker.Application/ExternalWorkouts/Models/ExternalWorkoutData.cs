using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.ExternalWorkouts.Models;

public class ExternalWorkoutData
{
    public WorkoutSource Source { get; set; }

    public string? ExternalId { get; set; }

    public string? Name { get; set; }

    public string? OriginalFileName { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public decimal? SourceDistanceKm { get; set; }

    public int? SourceDurationSeconds { get; set; }

    public List<ExternalWorkoutPoint> Points { get; set; } = new();

    public bool HasRouteData => Points.Count > 0;
}