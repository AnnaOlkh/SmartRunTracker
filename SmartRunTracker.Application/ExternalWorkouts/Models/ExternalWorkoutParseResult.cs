namespace SmartRunTracker.Application.ExternalWorkouts.Models;

public class ExternalWorkoutParseResult
{
    public bool IsSuccess { get; set; }

    public ExternalWorkoutData? Data { get; set; }

    public string? ErrorMessage { get; set; }

    public static ExternalWorkoutParseResult Success(ExternalWorkoutData data)
    {
        return new ExternalWorkoutParseResult
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static ExternalWorkoutParseResult Failure(string errorMessage)
    {
        return new ExternalWorkoutParseResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}