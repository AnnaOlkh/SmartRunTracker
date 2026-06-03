using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.Workouts.Analysis;

public interface IPlannedVsActualAnalyzer
{
    PlannedVsActualDto? Analyze(Workout workout);
}