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
}