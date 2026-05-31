namespace SmartRunTracker.Application.RunningGoals;

public interface IRunningGoalService
{
    Task<RunningGoalDto?> GetActiveAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningGoalDto>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<RunningGoalDto> CreateAsync(
        int userId,
        CreateRunningGoalRequest request,
        CancellationToken cancellationToken = default);
}