using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.Workouts;

public interface IWorkoutRepository
{
    Task<IReadOnlyList<Workout>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Workout>> GetRecentAsync(
       int userId,
       DateTimeOffset from,
       DateTimeOffset to,
       CancellationToken cancellationToken = default);

    Task<Workout?> GetByIdAsync(
        int userId,
        int workoutId,
        CancellationToken cancellationToken = default);

    Task<Workout> AddAsync(
        Workout workout,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Workout workout,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Workout workout,
        CancellationToken cancellationToken = default);
}