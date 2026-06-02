namespace SmartRunTracker.Application.Workouts.Analysis;

public sealed record WorkoutInsightDto(
    string Title,
    string Message,
    WorkoutInsightSeverity Severity);