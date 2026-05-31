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

    Task DeactivateActiveGoalsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<RunningGoal> AddAsync(
        RunningGoal goal,
        CancellationToken cancellationToken = default);
}