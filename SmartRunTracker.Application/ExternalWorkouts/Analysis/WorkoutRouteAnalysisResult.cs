namespace SmartRunTracker.Application.ExternalWorkouts.Analysis;

public class WorkoutRouteAnalysisResult
{
    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset EndedAt { get; set; }

    public decimal TotalDistanceKm { get; set; }

    public int DurationSeconds { get; set; }

    public int AveragePaceSecondsPerKm { get; set; }

    public List<AnalyzedRoutePoint> RoutePoints { get; set; } = new();

    public List<AnalyzedWorkoutSplit> Splits { get; set; } = new();

    public static WorkoutRouteAnalysisResult Success(
        DateTimeOffset startedAt,
        DateTimeOffset endedAt,
        decimal totalDistanceKm,
        int durationSeconds,
        int averagePaceSecondsPerKm,
        List<AnalyzedRoutePoint> routePoints,
        List<AnalyzedWorkoutSplit> splits)
    {
        return new WorkoutRouteAnalysisResult
        {
            IsSuccess = true,
            StartedAt = startedAt,
            EndedAt = endedAt,
            TotalDistanceKm = totalDistanceKm,
            DurationSeconds = durationSeconds,
            AveragePaceSecondsPerKm = averagePaceSecondsPerKm,
            RoutePoints = routePoints,
            Splits = splits
        };
    }

    public static WorkoutRouteAnalysisResult Failure(string errorMessage)
    {
        return new WorkoutRouteAnalysisResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}