namespace SmartRunTracker.Application.ExternalWorkouts.Analysis;

public class AnalyzedWorkoutSplit
{
    public int SplitNumber { get; set; }

    public decimal DistanceKm { get; set; }

    public int DurationSeconds { get; set; }

    public int AveragePaceSecondsPerKm { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }
}