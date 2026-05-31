using SmartRunTracker.Application.RunnerProfiles;
using SmartRunTracker.Application.RunningGoals;
using SmartRunTracker.Application.Workouts;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.TrainingPlans;

public sealed class TrainingPlanService : ITrainingPlanService
{
    private readonly ITrainingPlanRepository _trainingPlanRepository;
    private readonly IRunningGoalRepository _runningGoalRepository;
    private readonly IRunnerProfileRepository _runnerProfileRepository;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ITrainingWeekGenerator _trainingWeekGenerator;

    public TrainingPlanService(
        ITrainingPlanRepository trainingPlanRepository,
        IRunningGoalRepository runningGoalRepository,
        IRunnerProfileRepository runnerProfileRepository,
        IWorkoutRepository workoutRepository,
        ITrainingWeekGenerator trainingWeekGenerator)
    {
        _trainingPlanRepository = trainingPlanRepository;
        _runningGoalRepository = runningGoalRepository;
        _runnerProfileRepository = runnerProfileRepository;
        _workoutRepository = workoutRepository;
        _trainingWeekGenerator = trainingWeekGenerator;
    }

    public async Task<TrainingWeekDto> GenerateWeekAsync(
        int userId,
        GenerateTrainingWeekRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingWeek = await _trainingPlanRepository.GetByWeekStartDateAsync(
            userId,
            request.WeekStartDate,
            cancellationToken);

        if (existingWeek is not null)
        {
            return ToDto(existingWeek);
        }

        var activeGoal = await _runningGoalRepository.GetActiveAsync(
            userId,
            cancellationToken);

        if (activeGoal is null)
        {
            throw new InvalidOperationException(
                "Active running goal must exist before generating a training week.");
        }

        var runnerProfile = await GetOrCreateRunnerProfileAsync(
            userId,
            cancellationToken);

        var recentFrom = ToUtcDateTimeOffset(request.WeekStartDate.AddDays(-14));
        var recentTo = ToUtcDateTimeOffset(request.WeekStartDate);

        var recentWorkouts = await _workoutRepository.GetRecentAsync(
            userId,
            recentFrom,
            recentTo,
            cancellationToken);

        var recentPlannedSessions = await _trainingPlanRepository.GetRecentPlannedSessionsAsync(
            userId,
            recentFrom,
            recentTo,
            cancellationToken);

        var generatedWeek = _trainingWeekGenerator.Generate(
            new TrainingWeekGenerationInput(
                userId,
                activeGoal,
                runnerProfile,
                recentWorkouts,
                recentPlannedSessions,
                request.WeekStartDate));

        var trainingWeek = new TrainingWeek
        {
            UserId = userId,
            RunningGoalId = activeGoal.Id,
            WeekStartDate = request.WeekStartDate,
            TargetWorkoutCount = generatedWeek.TargetWorkoutCount,
            AdjustmentMode = generatedWeek.AdjustmentMode,
            TargetWeekDurationSeconds = generatedWeek.TargetWeekDurationSeconds,
            Status = TrainingWeekStatus.Draft,
            Explanation = generatedWeek.Explanation,
            GeneratedAt = DateTimeOffset.UtcNow,
            PlannedSessions = generatedWeek.Sessions
                .Select(session => new PlannedSession
                {
                    Type = session.Type,
                    Intensity = session.Intensity,
                    TargetDurationSeconds = session.TargetDurationSeconds,
                    TargetDistanceKm = session.TargetDistanceKm,
                    TargetPaceSecondsPerKm = session.TargetPaceSecondsPerKm,
                    ScheduledFor = null,
                    Status = PlannedSessionStatus.Unscheduled,
                    Notes = session.Notes,
                    Reason = session.Reason,
                    SortOrder = session.SortOrder
                })
                .ToList()
        };

        var createdWeek = await _trainingPlanRepository.AddWeekAsync(
            trainingWeek,
            cancellationToken);

        return ToDto(createdWeek);
    }

    public async Task<IReadOnlyList<TrainingWeekDto>> GetWeeksAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var weeks = await _trainingPlanRepository.GetWeeksAsync(
            userId,
            cancellationToken);

        return weeks
            .Select(ToDto)
            .ToList();
    }

    public async Task<TrainingWeekDto?> GetWeekByIdAsync(
        int userId,
        int trainingWeekId,
        CancellationToken cancellationToken = default)
    {
        var week = await _trainingPlanRepository.GetWeekByIdAsync(
            userId,
            trainingWeekId,
            cancellationToken);

        return week is null ? null : ToDto(week);
    }

    public async Task<PlannedSessionDto?> ScheduleSessionAsync(
        int userId,
        int plannedSessionId,
        SchedulePlannedSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _trainingPlanRepository.GetPlannedSessionByIdAsync(
            userId,
            plannedSessionId,
            cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status == PlannedSessionStatus.Completed)
        {
            throw new InvalidOperationException(
                "Completed planned session cannot be rescheduled.");
        }

        session.ScheduledFor = request.ScheduledFor;
        session.Status = PlannedSessionStatus.Scheduled;

        await _trainingPlanRepository.UpdatePlannedSessionAsync(
            session,
            cancellationToken);

        return ToDto(session);
    }

    public async Task<PlannedSessionDto?> SkipSessionAsync(
        int userId,
        int plannedSessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _trainingPlanRepository.GetPlannedSessionByIdAsync(
            userId,
            plannedSessionId,
            cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status == PlannedSessionStatus.Completed)
        {
            throw new InvalidOperationException(
                "Completed planned session cannot be skipped.");
        }

        session.Status = PlannedSessionStatus.Skipped;

        await _trainingPlanRepository.UpdatePlannedSessionAsync(
            session,
            cancellationToken);

        return ToDto(session);
    }

    private async Task<RunnerProfile> GetOrCreateRunnerProfileAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var existingProfile = await _runnerProfileRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (existingProfile is not null)
        {
            return existingProfile;
        }

        var profile = new RunnerProfile
        {
            UserId = userId,
            PreferredWorkoutsPerWeek = 3,
            TrainingDayPreferenceMode = TrainingDayPreferenceMode.AnyDay,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return await _runnerProfileRepository.AddAsync(
            profile,
            cancellationToken);
    }

    private static TrainingWeekDto ToDto(TrainingWeek week)
    {
        var plannedSessions = week.PlannedSessions
            .OrderBy(session => session.SortOrder)
            .Select(ToDto)
            .ToList();

        return new TrainingWeekDto(
            week.Id,
            week.UserId,
            week.RunningGoalId,
            week.WeekStartDate,
            week.TargetWorkoutCount,
            week.AdjustmentMode,
            week.TargetWeekDurationSeconds,
            week.Status,
            week.Explanation,
            week.GeneratedAt,
            plannedSessions
        );
    }

    private static PlannedSessionDto ToDto(PlannedSession session)
    {
        return new PlannedSessionDto(
            session.Id,
            session.TrainingWeekId,
            session.Type,
            session.Intensity,
            session.TargetDurationSeconds,
            session.TargetDistanceKm,
            session.TargetPaceSecondsPerKm,
            session.ScheduledFor,
            session.Status,
            session.Notes,
            session.Reason,
            session.SortOrder
        );
    }

    private static DateTimeOffset ToUtcDateTimeOffset(DateOnly date)
    {
        return new DateTimeOffset(
            date.ToDateTime(TimeOnly.MinValue),
            TimeSpan.Zero);
    }
}