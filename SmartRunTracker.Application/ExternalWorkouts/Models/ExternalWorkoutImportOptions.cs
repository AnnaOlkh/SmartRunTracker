using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.ExternalWorkouts.Models;

public class ExternalWorkoutImportOptions
{
    public int UserId { get; set; }

    public int? PlannedSessionId { get; set; }

    public WorkoutType? WorkoutType { get; set; }

    public int? Rpe { get; set; }

    public string? Notes { get; set; }
}