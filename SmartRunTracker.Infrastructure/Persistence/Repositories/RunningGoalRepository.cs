using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.RunningGoals;
using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Infrastructure.Persistence.Repositories;

public sealed class RunningGoalRepository : IRunningGoalRepository
{
    private readonly AppDbContext _dbContext;

    public RunningGoalRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RunningGoal?> GetActiveAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RunningGoals
            .AsNoTracking()
            .FirstOrDefaultAsync(
                goal => goal.UserId == userId && goal.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RunningGoal>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RunningGoals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId)
            .OrderByDescending(goal => goal.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<RunningGoal?> GetByIdAsync(
        int userId,
        int goalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RunningGoals
            .FirstOrDefaultAsync(
                goal => goal.Id == goalId && goal.UserId == userId,
                cancellationToken);
    }

    public async Task<bool> EquivalentGoalExistsAsync(
        int userId,
        decimal targetDistanceKm,
        int targetPaceSecondsPerKm,
        DateOnly? goalDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RunningGoals
            .AnyAsync(
                goal =>
                    goal.UserId == userId
                    && goal.TargetDistanceKm == targetDistanceKm
                    && goal.TargetPaceSecondsPerKm == targetPaceSecondsPerKm
                    && goal.GoalDate == goalDate,
                cancellationToken);
    }
    public async Task DeactivateActiveGoalsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var activeGoals = await _dbContext.RunningGoals
            .Where(goal => goal.UserId == userId && goal.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var goal in activeGoals)
        {
            goal.IsActive = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RunningGoal> AddAsync(
        RunningGoal goal,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RunningGoals.Add(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return goal;
    }
    public async Task UpdateAsync(
    RunningGoal goal,
    CancellationToken cancellationToken = default)
    {
        _dbContext.RunningGoals.Update(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteAsync(
       RunningGoal goal,
       CancellationToken cancellationToken = default)
    {
        _dbContext.RunningGoals.Remove(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}