using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Domain.Entities;

public class Workout
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? PlannedSessionId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public decimal DistanceKm { get; set; }

    public int DurationSeconds { get; set; }

    public int Rpe { get; set; }

    public WorkoutType Type { get; set; }

    public WorkoutSource Source { get; set; } = WorkoutSource.Manual;

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;

    public PlannedSession? PlannedSession { get; set; }
}