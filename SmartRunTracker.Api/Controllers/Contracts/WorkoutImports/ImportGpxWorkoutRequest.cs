using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Api.Contracts.WorkoutImports;

public sealed class ImportGpxWorkoutRequest
{
    public IFormFile? File { get; set; }

    public int? PlannedSessionId { get; set; }

    public WorkoutType? WorkoutType { get; set; }

    public int? Rpe { get; set; }

    public string? Notes { get; set; }
}