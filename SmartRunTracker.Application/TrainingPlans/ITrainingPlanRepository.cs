using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.TrainingPlans;

public interface ITrainingPlanRepository
{
    Task<TrainingWeek?> GetByWeekStartDateAsync(
        int userId,
        DateOnly weekStartDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrainingWeek>> GetWeeksAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<TrainingWeek?> GetWeekByIdAsync(
        int userId,
        int trainingWeekId,
        CancellationToken cancellationToken = default);

    Task<TrainingWeek> AddWeekAsync(
        TrainingWeek trainingWeek,
        CancellationToken cancellationToken = default);

    Task<PlannedSession?> GetPlannedSessionByIdAsync(
        int userId,
        int plannedSessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlannedSession>> GetRecentPlannedSessionsAsync(
        int userId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task UpdatePlannedSessionAsync(
        PlannedSession plannedSession,
        CancellationToken cancellationToken = default);
}