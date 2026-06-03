using SmartRunTracker.Application.TrainingPlans;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;
using SmartRunTracker.Application.Workouts.Analysis;

namespace SmartRunTracker.Application.Workouts;

public sealed class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ITrainingPlanRepository _trainingPlanRepository;
    private readonly IPlannedVsActualAnalyzer _plannedVsActualAnalyzer;
    private readonly IWorkoutInsightBuilder _workoutInsightBuilder;

    public WorkoutService(
    IWorkoutRepository workoutRepository,
    ITrainingPlanRepository trainingPlanRepository,
    IPlannedVsActualAnalyzer plannedVsActualAnalyzer,
    IWorkoutInsightBuilder workoutInsightBuilder)
    {
        _workoutRepository = workoutRepository;
        _trainingPlanRepository = trainingPlanRepository;
        _plannedVsActualAnalyzer = plannedVsActualAnalyzer;
        _workoutInsightBuilder = workoutInsightBuilder;
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

    public async Task<WorkoutDetailsDto?> GetDetailsAsync(
    int userId,
    int workoutId,
    CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetDetailsByIdAsync(
            userId,
            workoutId,
            cancellationToken);

        return workout is null ? null : ToDetailsDto(workout);
    }
    public async Task<WorkoutDto> CreateAsync(
        int userId,
        CreateWorkoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        PlannedSession? plannedSession = null;

        if (request.PlannedSessionId is not null)
        {
            plannedSession = await _trainingPlanRepository.GetPlannedSessionByIdAsync(
                userId,
                request.PlannedSessionId.Value,
                cancellationToken);

            if (plannedSession is null)
            {
                throw new ArgumentException(
                    "Planned session was not found for this user.");
            }

            if (plannedSession.Status == PlannedSessionStatus.Completed)
            {
                throw new ArgumentException(
                    "Planned session is already completed.");
            }
        }

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

        if (plannedSession is not null)
        {
            plannedSession.Status = PlannedSessionStatus.Completed;
            plannedSession.ScheduledFor ??= request.StartedAt;

            await _trainingPlanRepository.UpdatePlannedSessionAsync(
                plannedSession,
                cancellationToken);
        }

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

        if (workout.PlannedSessionId is not null)
        {
            var plannedSession = await _trainingPlanRepository.GetPlannedSessionByIdAsync(
                userId,
                workout.PlannedSessionId.Value,
                cancellationToken);

            if (plannedSession is not null
                && plannedSession.Status == PlannedSessionStatus.Completed)
            {
                plannedSession.Status = ResolveStatusAfterWorkoutDelete(
                    plannedSession.ScheduledFor);

                await _trainingPlanRepository.UpdatePlannedSessionAsync(
                    plannedSession,
                    cancellationToken);
            }
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
    private static PlannedSessionStatus ResolveStatusAfterWorkoutDelete(
    DateTimeOffset? scheduledFor)
    {
        if (scheduledFor is null)
        {
            return PlannedSessionStatus.Unscheduled;
        }

        if (scheduledFor < DateTimeOffset.UtcNow)
        {
            return PlannedSessionStatus.Skipped;
        }

        return PlannedSessionStatus.Scheduled;
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
    public async Task<IReadOnlyList<PlannedSessionDto>> GetAvailablePlannedSessionsAsync(
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
            return Array.Empty<PlannedSessionDto>();
        }

        var workoutDate = DateOnly.FromDateTime(workout.StartedAt.UtcDateTime);

        var weeks = await _trainingPlanRepository.GetWeeksAsync(
            userId,
            cancellationToken);

        var week = weeks.FirstOrDefault(candidate =>
            candidate.WeekStartDate <= workoutDate &&
            candidate.WeekStartDate.AddDays(7) > workoutDate);

        if (week is null)
        {
            return Array.Empty<PlannedSessionDto>();
        }

        return week.PlannedSessions
            .OrderBy(session => session.SortOrder)
            .Select(ToTrainingPlanPlannedSessionDto)
            .ToList();
    }
    public async Task<WorkoutDetailsDto?> LinkPlannedSessionAsync(
    int userId,
    int workoutId,
    LinkWorkoutPlannedSessionRequest request,
    CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(
            userId,
            workoutId,
            cancellationToken);

        if (workout is null)
        {
            return null;
        }

        var plannedSession = await _trainingPlanRepository.GetPlannedSessionByIdAsync(
            userId,
            request.PlannedSessionId,
            cancellationToken);

        if (plannedSession is null)
        {
            throw new ArgumentException("Planned session was not found for this user.");
        }

        var workoutDate = DateOnly.FromDateTime(workout.StartedAt.UtcDateTime);
        var weekStart = plannedSession.TrainingWeek.WeekStartDate;
        var weekEndExclusive = weekStart.AddDays(7);

        if (workoutDate < weekStart || workoutDate >= weekEndExclusive)
        {
            throw new ArgumentException(
                "Selected planned session does not belong to the workout week.");
        }

        if (plannedSession.Status == PlannedSessionStatus.Completed &&
            workout.PlannedSessionId != plannedSession.Id)
        {
            throw new ArgumentException("Planned session is already completed.");
        }

        if (workout.PlannedSessionId is not null &&
            workout.PlannedSessionId != plannedSession.Id)
        {
            var previousSession = await _trainingPlanRepository.GetPlannedSessionByIdAsync(
                userId,
                workout.PlannedSessionId.Value,
                cancellationToken);

            if (previousSession is not null &&
                previousSession.Status == PlannedSessionStatus.Completed)
            {
                previousSession.Status = ResolveStatusAfterWorkoutDelete(
                    previousSession.ScheduledFor);

                await _trainingPlanRepository.UpdatePlannedSessionAsync(
                    previousSession,
                    cancellationToken);
            }
        }

        workout.PlannedSessionId = plannedSession.Id;

        plannedSession.Status = PlannedSessionStatus.Completed;
        plannedSession.ScheduledFor ??= workout.StartedAt;

        await _workoutRepository.UpdateAsync(workout, cancellationToken);

        await _trainingPlanRepository.UpdatePlannedSessionAsync(
            plannedSession,
            cancellationToken);

        return await GetDetailsAsync(userId, workoutId, cancellationToken);
    }
    private static PlannedSessionDto ToTrainingPlanPlannedSessionDto(
    PlannedSession plannedSession)
    {
        return new PlannedSessionDto(
            plannedSession.Id,
            plannedSession.TrainingWeekId,
            plannedSession.Type,
            plannedSession.Intensity,
            plannedSession.TargetDurationSeconds,
            plannedSession.TargetDistanceKm,
            plannedSession.TargetPaceSecondsPerKm,
            plannedSession.ScheduledFor,
            plannedSession.Status,
            plannedSession.Notes,
            plannedSession.Reason,
            plannedSession.SortOrder);
    }
    private WorkoutDetailsDto ToDetailsDto(Workout workout)
    {
        var routePoints = workout.RoutePoints
            .OrderBy(point => point.Order)
            .Select(ToRoutePointDto)
            .ToList();

        var splits = workout.Splits
            .OrderBy(split => split.SplitNumber)
            .Select(ToSplitDto)
            .ToList();

        var plannedSession = workout.PlannedSession is null
            ? null
            : ToPlannedSessionDto(workout.PlannedSession);

        var plannedVsActual = _plannedVsActualAnalyzer.Analyze(workout);
        var insights = _workoutInsightBuilder.Build(workout, plannedVsActual);

        return new WorkoutDetailsDto(
            ToDto(workout),
            plannedSession,
            plannedVsActual,
            insights,
            routePoints,
            splits);
    }

    private static WorkoutRoutePointDto ToRoutePointDto(WorkoutRoutePoint point)
    {
        return new WorkoutRoutePointDto(
            point.Id,
            point.Order,
            point.Latitude,
            point.Longitude,
            point.ElevationMeters,
            point.RecordedAt,
            point.DistanceFromStartMeters,
            point.SecondsFromStart,
            point.PaceSecondsPerKm);
    }

    private static WorkoutSplitDto ToSplitDto(WorkoutSplit split)
    {
        return new WorkoutSplitDto(
            split.Id,
            split.SplitNumber,
            split.DistanceKm,
            split.DurationSeconds,
            split.AveragePaceSecondsPerKm,
            split.StartedAt,
            split.EndedAt);
    }

    private static WorkoutPlannedSessionDto ToPlannedSessionDto(PlannedSession plannedSession)
    {
        return new WorkoutPlannedSessionDto(
            plannedSession.Id,
            plannedSession.Type,
            plannedSession.Intensity,
            plannedSession.TargetDurationSeconds,
            plannedSession.TargetDistanceKm,
            plannedSession.TargetPaceSecondsPerKm,
            plannedSession.ScheduledFor,
            plannedSession.Status);
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