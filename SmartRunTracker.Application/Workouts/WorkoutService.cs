using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts;

public sealed class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;

    public WorkoutService(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<IReadOnlyList<WorkoutDto>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetAllAsync(userId, cancellationToken);

        return workouts
            .Select(ToDto)
            .ToList();
    }

    public async Task<WorkoutDto?> GetByIdAsync(
        int userId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(
            userId,
            workoutId,
            cancellationToken);

        return workout is null ? null : ToDto(workout);
    }

    public async Task<WorkoutDto> CreateAsync(
        int userId,
        CreateWorkoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var workout = new Workout
        {
            UserId = userId,
            PlannedSessionId = request.PlannedSessionId,
            StartedAt = request.StartedAt,
            DistanceKm = request.DistanceKm,
            DurationSeconds = request.DurationSeconds,
            Rpe = request.Rpe,
            Type = request.Type,
            Source = WorkoutSource.Manual,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createdWorkout = await _workoutRepository.AddAsync(
            workout,
            cancellationToken);

        return ToDto(createdWorkout);
    }

    public async Task<WorkoutDto?> UpdateAsync(
        int userId,
        int workoutId,
        UpdateWorkoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdateRequest(request);

        var workout = await _workoutRepository.GetByIdAsync(
            userId,
            workoutId,
            cancellationToken);

        if (workout is null)
        {
            return null;
        }

        workout.StartedAt = request.StartedAt;
        workout.DistanceKm = request.DistanceKm;
        workout.DurationSeconds = request.DurationSeconds;
        workout.Rpe = request.Rpe;
        workout.Type = request.Type;
        workout.Notes = request.Notes;

        await _workoutRepository.UpdateAsync(workout, cancellationToken);

        return ToDto(workout);
    }

    public async Task<bool> DeleteAsync(
        int userId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(
            userId,
            workoutId,
            cancellationToken);

        if (workout is null)
        {
            return false;
        }

        await _workoutRepository.DeleteAsync(workout, cancellationToken);

        return true;
    }

    private static void ValidateCreateRequest(CreateWorkoutRequest request)
    {
        ValidateWorkoutValues(
            request.DistanceKm,
            request.DurationSeconds,
            request.Rpe);
    }

    private static void ValidateUpdateRequest(UpdateWorkoutRequest request)
    {
        ValidateWorkoutValues(
            request.DistanceKm,
            request.DurationSeconds,
            request.Rpe);
    }

    private static void ValidateWorkoutValues(
        decimal distanceKm,
        int durationSeconds,
        int rpe)
    {
        if (distanceKm <= 0)
        {
            throw new ArgumentException("Distance must be greater than zero.");
        }

        if (durationSeconds <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.");
        }

        if (rpe is < 1 or > 10)
        {
            throw new ArgumentException("RPE must be between 1 and 10.");
        }
    }

    private static WorkoutDto ToDto(Workout workout)
    {
        var averagePaceSecondsPerKm = CalculateAveragePaceSecondsPerKm(workout);
        var sessionLoad = CalculateSessionLoad(workout);

        return new WorkoutDto(
            workout.Id,
            workout.UserId,
            workout.PlannedSessionId,
            workout.StartedAt,
            workout.DistanceKm,
            workout.DurationSeconds,
            averagePaceSecondsPerKm,
            workout.Rpe,
            sessionLoad,
            workout.Type,
            workout.Source,
            workout.Notes,
            workout.CreatedAt
        );
    }

    private static int CalculateAveragePaceSecondsPerKm(Workout workout)
    {
        if (workout.DistanceKm <= 0)
        {
            return 0;
        }

        return (int)Math.Round(
            (double)(workout.DurationSeconds / workout.DistanceKm),
            MidpointRounding.AwayFromZero);
    }

    private static int CalculateSessionLoad(Workout workout)
    {
        return (int)Math.Round(
            workout.DurationSeconds / 60.0 * workout.Rpe,
            MidpointRounding.AwayFromZero);
    }
}