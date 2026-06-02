namespace SmartRunTracker.Domain.Entities;

public class WorkoutSplit
{
    public int Id { get; set; }

    public int WorkoutId { get; set; }

    public int SplitNumber { get; set; }

    public decimal DistanceKm { get; set; }

    public int DurationSeconds { get; set; }

    public int AveragePaceSecondsPerKm { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public Workout Workout { get; set; } = null!;
}