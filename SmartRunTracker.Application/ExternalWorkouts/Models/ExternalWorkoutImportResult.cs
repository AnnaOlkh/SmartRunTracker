using SmartRunTracker.Application.Workouts;

namespace SmartRunTracker.Application.ExternalWorkouts.Models;

public class ExternalWorkoutImportResult
{
    public bool IsSuccess { get; set; }

    public WorkoutDto? Workout { get; set; }

    public string? ErrorMessage { get; set; }

    public static ExternalWorkoutImportResult Success(WorkoutDto workout)
    {
        return new ExternalWorkoutImportResult
        {
            IsSuccess = true,
            Workout = workout
        };
    }

    public static ExternalWorkoutImportResult Failure(string errorMessage)
    {
        return new ExternalWorkoutImportResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}