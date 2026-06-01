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

        var duplicateExists = await _runningGoalRepository.EquivalentGoalExistsAsync(
            userId,
            request.TargetDistanceKm,
            request.TargetPaceSecondsPerKm,
            request.GoalDate,
            cancellationToken);

        if (duplicateExists)
        {
            throw new ArgumentException(
                "Running goal with the same distance, pace and date already exists.");
        }

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

    public async Task<RunningGoalDto?> ActivateAsync(
        int userId,
        int goalId,
        CancellationToken cancellationToken = default)
    {
        var goal = await _runningGoalRepository.GetByIdAsync(
            userId,
            goalId,
            cancellationToken);

        if (goal is null)
        {
            return null;
        }

        ValidateGoalDateIsNotPast(goal.GoalDate);

        await _runningGoalRepository.DeactivateActiveGoalsAsync(
            userId,
            cancellationToken);

        goal.IsActive = true;

        await _runningGoalRepository.UpdateAsync(goal, cancellationToken);

        return ToDto(goal);
    }

    public async Task<bool> DeleteAsync(
        int userId,
        int goalId,
        CancellationToken cancellationToken = default)
    {
        var goal = await _runningGoalRepository.GetByIdAsync(
            userId,
            goalId,
            cancellationToken);

        if (goal is null)
        {
            return false;
        }

        await _runningGoalRepository.DeleteAsync(goal, cancellationToken);

        return true;
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

        ValidateGoalDateIsNotPast(request.GoalDate);
    }

    private static void ValidateGoalDateIsNotPast(DateOnly? goalDate)
    {
        if (goalDate is null)
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (goalDate < today)
        {
            throw new ArgumentException(
                "Goal date cannot be in the past.");
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