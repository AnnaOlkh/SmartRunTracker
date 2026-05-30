using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Domain.Entities;

public class PlannedSession
{
    public int Id { get; set; }

    public int TrainingWeekId { get; set; }

    public WorkoutType Type { get; set; }

    public WorkoutIntensity Intensity { get; set; }

    public int TargetDurationSeconds { get; set; }

    public decimal? TargetDistanceKm { get; set; }

    public int? TargetPaceSecondsPerKm { get; set; }

    public DateTimeOffset? ScheduledFor { get; set; }

    public PlannedSessionStatus Status { get; set; } = PlannedSessionStatus.Unscheduled;

    public string? Notes { get; set; }

    public string? Reason { get; set; }

    public int SortOrder { get; set; }

    public TrainingWeek TrainingWeek { get; set; } = null!;

    public Workout? CompletedWorkout { get; set; }
}