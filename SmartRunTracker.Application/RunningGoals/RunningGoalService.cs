using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.RunningGoals;

public sealed class RunningGoalService : IRunningGoalService
{
    private readonly IRunningGoalRepository _runningGoalRepository;

    public RunningGoalService(IRunningGoalRepository runningGoalRepository)
    {
        _runningGoalRepository = runningGoalRepository;
    }

    public async Task<RunningGoalDto?> GetActiveAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var goal = await _runningGoalRepository.GetActiveAsync(
            userId,
            cancellationToken);

        return goal is null ? null : ToDto(goal);
    }

    public async Task<IReadOnlyList<RunningGoalDto>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var goals = await _runningGoalRepository.GetAllAsync(
            userId,
            cancellationToken);

        return goals
            .Select(ToDto)
            .ToList();
    }

    public async Task<RunningGoalDto> CreateAsync(
        int userId,
        CreateRunningGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        await _runningGoalRepository.DeactivateActiveGoalsAsync(
            userId,
            cancellationToken);

        var goal = new RunningGoal
        {
            UserId = userId,
            TargetDistanceKm = request.TargetDistanceKm,
            TargetPaceSecondsPerKm = request.TargetPaceSecondsPerKm,
            GoalDate = request.GoalDate,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createdGoal = await _runningGoalRepository.AddAsync(
            goal,
            cancellationToken);

        return ToDto(createdGoal);
    }

    private static void ValidateRequest(CreateRunningGoalRequest request)
    {
        if (request.TargetDistanceKm <= 0)
        {
            throw new ArgumentException(
                "Target distance must be greater than zero.");
        }

        if (request.TargetPaceSecondsPerKm <= 0)
        {
            throw new ArgumentException(
                "Target pace must be greater than zero.");
        }
    }

    private static RunningGoalDto ToDto(RunningGoal goal)
    {
        var targetFinishSeconds = (int)Math.Round(
            goal.TargetDistanceKm * goal.TargetPaceSecondsPerKm,
            MidpointRounding.AwayFromZero);

        return new RunningGoalDto(
            goal.Id,
            goal.UserId,
            goal.TargetDistanceKm,
            goal.TargetPaceSecondsPerKm,
            targetFinishSeconds,
            goal.GoalDate,
            goal.IsActive,
            goal.CreatedAt
        );
    }
}