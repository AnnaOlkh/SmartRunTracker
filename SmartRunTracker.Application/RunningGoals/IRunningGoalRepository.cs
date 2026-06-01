using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.RunningGoals;

public interface IRunningGoalRepository
{
    Task<RunningGoal?> GetActiveAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningGoal>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<RunningGoal?> GetByIdAsync(
        int userId,
        int goalId,
        CancellationToken cancellationToken = default);

    Task<bool> EquivalentGoalExistsAsync(
        int userId,
        decimal targetDistanceKm,
        int targetPaceSecondsPerKm,
        DateOnly? goalDate,
        CancellationToken cancellationToken = default);

    Task DeactivateActiveGoalsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<RunningGoal> AddAsync(
        RunningGoal goal,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(
        RunningGoal goal,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        RunningGoal goal,
        CancellationToken cancellationToken = default);

}