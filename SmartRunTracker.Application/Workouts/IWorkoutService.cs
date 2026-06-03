using SmartRunTracker.Application.TrainingPlans;

namespace SmartRunTracker.Application.Workouts;

public interface IWorkoutService
{
    Task<IReadOnlyList<WorkoutDto>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<WorkoutDto?> GetByIdAsync(
        int userId,
        int workoutId,
        CancellationToken cancellationToken = default);
    Task<WorkoutDetailsDto?> GetDetailsAsync(
        int userId,
        int workoutId,
        CancellationToken cancellationToken = default);

    Task<WorkoutDto> CreateAsync(
        int userId,
        CreateWorkoutRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkoutDto?> UpdateAsync(
        int userId,
        int workoutId,
        UpdateWorkoutRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int userId,
        int workoutId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlannedSessionDto>> GetAvailablePlannedSessionsAsync(
    int userId,
    int workoutId,
    CancellationToken cancellationToken = default);

    Task<WorkoutDetailsDto?> LinkPlannedSessionAsync(
        int userId,
        int workoutId,
        LinkWorkoutPlannedSessionRequest request,
        CancellationToken cancellationToken = default);
}