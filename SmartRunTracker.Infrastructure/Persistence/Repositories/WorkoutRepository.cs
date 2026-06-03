using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.Workouts;
using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Infrastructure.Persistence.Repositories;

public sealed class WorkoutRepository : IWorkoutRepository
{
    private readonly AppDbContext _dbContext;

    public WorkoutRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Workout>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId)
            .OrderByDescending(workout => workout.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Workout>> GetRecentAsync(
       int userId,
       DateTimeOffset from,
       DateTimeOffset to,
       CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId)
            .Where(workout => workout.StartedAt >= from && workout.StartedAt < to)
            .OrderByDescending(workout => workout.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Workout?> GetByIdAsync(
        int userId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .FirstOrDefaultAsync(
                workout => workout.Id == workoutId && workout.UserId == userId,
                cancellationToken);
    }
    public async Task<Workout?> GetDetailsByIdAsync(
    int userId,
    int workoutId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(workout => workout.RoutePoints)
            .Include(workout => workout.Splits)
            .Include(workout => workout.PlannedSession)
            .FirstOrDefaultAsync(
                workout => workout.Id == workoutId && workout.UserId == userId,
                cancellationToken);
    }
    public async Task<Workout> AddAsync(
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Workouts.Add(workout);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return workout;
    }

    public async Task UpdateAsync(
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Workouts.Update(workout);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Workouts.Remove(workout);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}