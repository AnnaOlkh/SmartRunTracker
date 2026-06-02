using SmartRunTracker.Application.ExternalWorkouts.Analysis;
using SmartRunTracker.Application.ExternalWorkouts.Models;

namespace SmartRunTracker.Application.ExternalWorkouts.Abstractions;

public interface IWorkoutRouteAnalyzer
{
    WorkoutRouteAnalysisResult Analyze(ExternalWorkoutData workoutData);
}