using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.TrainingPlans;

public sealed record TrainingWeekGenerationInput(
    int UserId,
    RunningGoal ActiveGoal,
    RunnerProfile RunnerProfile,
    IReadOnlyList<Workout> RecentWorkouts,
    IReadOnlyList<PlannedSession> RecentPlannedSessions,
    DateOnly WeekStartDate
);