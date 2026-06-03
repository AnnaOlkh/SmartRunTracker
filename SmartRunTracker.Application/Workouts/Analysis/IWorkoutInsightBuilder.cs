using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.Workouts.Analysis;

public interface IWorkoutInsightBuilder
{
    IReadOnlyList<WorkoutInsightDto> Build(
        Workout workout,
        PlannedVsActualDto? plannedVsActual);
}