using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.Auth;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;
using SmartRunTracker.Infrastructure.Persistence;

namespace SmartRunTracker.Infrastructure.Demo;

public sealed class DemoDataSeeder
{
    public const string DemoEmail = "demo@smartrun.local";
    public const string DemoPassword = "123456";

    private readonly AppDbContext _dbContext;
    private readonly IPasswordHashService _passwordHashService;

    public DemoDataSeeder(
        AppDbContext dbContext,
        IPasswordHashService passwordHashService)
    {
        _dbContext = dbContext;
        _passwordHashService = passwordHashService;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var user = await GetOrCreateDemoUserAsync(cancellationToken);

        await ClearDemoUserDataAsync(user.Id, cancellationToken);

        var profile = new RunnerProfile
        {
            UserId = user.Id,
            PreferredWorkoutsPerWeek = 3,
            TrainingDayPreferenceMode = TrainingDayPreferenceMode.AnyDay,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var goal = new RunningGoal
        {
            UserId = user.Id,
            TargetDistanceKm = 10.00m,
            TargetPaceSecondsPerKm = 330,
            GoalDate = new DateOnly(2026, 6, 28),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.RunnerProfiles.Add(profile);
        _dbContext.RunningGoals.Add(goal);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var weeks = CreateMay2026TrainingWeeks(user.Id, goal.Id);

        _dbContext.TrainingWeeks.AddRange(weeks);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var workouts = CreateActualWorkouts(user.Id, weeks);

        _dbContext.Workouts.AddRange(workouts);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<User> GetOrCreateDemoUserAsync(
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == DemoEmail, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        user = new User
        {
            DisplayName = "Demo Runner",
            Email = DemoEmail,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = _passwordHashService.HashPassword(
            user,
            DemoPassword);

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    private async Task ClearDemoUserDataAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        await _dbContext.WorkoutSplits
            .Where(x => x.Workout.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.WorkoutRoutePoints
            .Where(x => x.Workout.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Workouts
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.PlannedSessions
            .Where(x => x.TrainingWeek.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.TrainingWeeks
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.RunningGoals
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.RunnerAvailableDays
            .Where(x => x.RunnerProfile.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.RunnerProfiles
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static List<TrainingWeek> CreateMay2026TrainingWeeks(
        int userId,
        int goalId)
    {
        return new List<TrainingWeek>
        {
            CreateWeek(
                userId,
                goalId,
                new DateOnly(2026, 5, 4),
                AdjustmentMode.Maintain,
                "Initial May week generated from a neutral training history. The plan keeps volume controlled and includes one easy run, one recovery run and one long run.",
                new[]
                {
                    CompletedSession(
                        sortOrder: 1,
                        scheduledFor: Utc(2026, 5, 6, 18, 20),
                        type: WorkoutType.Easy,
                        intensity: WorkoutIntensity.Low,
                        targetDistanceKm: 6.00m,
                        targetDurationSeconds: 2280,
                        targetPaceSecondsPerKm: 380,
                        reason: "Easy aerobic run to build weekly volume without excessive intensity."),

                    CompletedSession(
                        sortOrder: 2,
                        scheduledFor: Utc(2026, 5, 8, 7, 30),
                        type: WorkoutType.Recovery,
                        intensity: WorkoutIntensity.Low,
                        targetDistanceKm: 4.00m,
                        targetDurationSeconds: 1680,
                        targetPaceSecondsPerKm: 420,
                        reason: "Short recovery session after the first easy run."),

                    SkippedSession(
                        sortOrder: 3,
                        scheduledFor: Utc(2026, 5, 10, 8, 00),
                        type: WorkoutType.Long,
                        intensity: WorkoutIntensity.Moderate,
                        targetDistanceKm: 8.00m,
                        targetDurationSeconds: 3040,
                        targetPaceSecondsPerKm: 380,
                        reason: "Long run was planned to extend endurance, but it was missed.")
                }),

            CreateWeek(
                userId,
                goalId,
                new DateOnly(2026, 5, 11),
                AdjustmentMode.Maintain,
                "The previous long run was missed, so the system keeps the week conservative instead of increasing load aggressively.",
                new[]
                {
                    CompletedSession(
                        sortOrder: 1,
                        scheduledFor: Utc(2026, 5, 13, 18, 35),
                        type: WorkoutType.Quality,
                        intensity: WorkoutIntensity.High,
                        targetDistanceKm: 7.00m,
                        targetDurationSeconds: 2520,
                        targetPaceSecondsPerKm: 360,
                        reason: "Quality session with faster controlled segments to support the 10 km goal."),

                    CompletedSession(
                        sortOrder: 2,
                        scheduledFor: Utc(2026, 5, 15, 7, 20),
                        type: WorkoutType.Easy,
                        intensity: WorkoutIntensity.Low,
                        targetDistanceKm: 5.50m,
                        targetDurationSeconds: 2145,
                        targetPaceSecondsPerKm: 390,
                        reason: "Easy mileage after the quality session."),

                    CompletedSession(
                        sortOrder: 3,
                        scheduledFor: Utc(2026, 5, 17, 8, 10),
                        type: WorkoutType.Long,
                        intensity: WorkoutIntensity.Moderate,
                        targetDistanceKm: 9.00m,
                        targetDurationSeconds: 3420,
                        targetPaceSecondsPerKm: 380,
                        reason: "Controlled long run to compensate for the previously skipped endurance session.")
                }),

            CreateWeek(
                userId,
                goalId,
                new DateOnly(2026, 5, 18),
                AdjustmentMode.Progress,
                "The previous week was completed successfully, so the system applies a moderate progression in total distance.",
                new[]
                {
                    CompletedSession(
                        sortOrder: 1,
                        scheduledFor: Utc(2026, 5, 20, 7, 50),
                        type: WorkoutType.Long,
                        intensity: WorkoutIntensity.Moderate,
                        targetDistanceKm: 10.00m,
                        targetDurationSeconds: 3700,
                        targetPaceSecondsPerKm: 370,
                        reason: "Main endurance session for the week, close to the target race distance."),

                    SkippedSession(
                        sortOrder: 2,
                        scheduledFor: Utc(2026, 5, 22, 18, 40),
                        type: WorkoutType.Recovery,
                        intensity: WorkoutIntensity.Low,
                        targetDistanceKm: 4.00m,
                        targetDurationSeconds: 1680,
                        targetPaceSecondsPerKm: 420,
                        reason: "Recovery run planned after the long session, but it was skipped."),

                    CompletedSession(
                        sortOrder: 3,
                        scheduledFor: Utc(2026, 5, 24, 8, 30),
                        type: WorkoutType.Easy,
                        intensity: WorkoutIntensity.Low,
                        targetDistanceKm: 6.00m,
                        targetDurationSeconds: 2340,
                        targetPaceSecondsPerKm: 390,
                        reason: "Easy run to keep weekly consistency without adding high intensity.")
                }),

            CreateWeek(
                userId,
                goalId,
                new DateOnly(2026, 5, 25),
                AdjustmentMode.Maintain,
                "Because one recovery run was skipped, the system maintains the load and avoids another strong progression.",
                new[]
                {
                    CompletedSession(
                        sortOrder: 1,
                        scheduledFor: Utc(2026, 5, 27, 18, 45),
                        type: WorkoutType.Quality,
                        intensity: WorkoutIntensity.High,
                        targetDistanceKm: 8.00m,
                        targetDurationSeconds: 2880,
                        targetPaceSecondsPerKm: 360,
                        reason: "Progression run with faster final segment to develop controlled speed endurance."),

                    ScheduledSession(
                        sortOrder: 2,
                        scheduledFor: Utc(2026, 5, 29, 7, 15),
                        type: WorkoutType.Easy,
                        intensity: WorkoutIntensity.Low,
                        targetDistanceKm: 5.00m,
                        targetDurationSeconds: 1950,
                        targetPaceSecondsPerKm: 390,
                        reason: "Easy run remains scheduled and has not been completed yet."),

                    CompletedSession(
                        sortOrder: 3,
                        scheduledFor: Utc(2026, 5, 31, 9, 15),
                        type: WorkoutType.Recovery,
                        intensity: WorkoutIntensity.Low,
                        targetDistanceKm: 4.50m,
                        targetDurationSeconds: 1890,
                        targetPaceSecondsPerKm: 420,
                        reason: "Light recovery run to close the May block.")
                })
        };
    }

    private static TrainingWeek CreateWeek(
        int userId,
        int goalId,
        DateOnly weekStartDate,
        AdjustmentMode adjustmentMode,
        string explanation,
        IEnumerable<PlannedSession> plannedSessions)
    {
        var sessions = plannedSessions.ToList();

        return new TrainingWeek
        {
            UserId = userId,
            RunningGoalId = goalId,
            WeekStartDate = weekStartDate,
            TargetWorkoutCount = sessions.Count,
            AdjustmentMode = adjustmentMode,
            TargetWeekDurationSeconds = sessions.Sum(x => x.TargetDurationSeconds),
            Status = TrainingWeekStatus.Completed,
            Explanation = explanation,
            GeneratedAt = DateTimeOffset.UtcNow,
            PlannedSessions = sessions
        };
    }

    private static PlannedSession CompletedSession(
        int sortOrder,
        DateTimeOffset scheduledFor,
        WorkoutType type,
        WorkoutIntensity intensity,
        decimal targetDistanceKm,
        int targetDurationSeconds,
        int targetPaceSecondsPerKm,
        string reason)
    {
        return PlannedSession(
            sortOrder,
            scheduledFor,
            type,
            intensity,
            targetDistanceKm,
            targetDurationSeconds,
            targetPaceSecondsPerKm,
            PlannedSessionStatus.Completed,
            reason);
    }

    private static PlannedSession SkippedSession(
        int sortOrder,
        DateTimeOffset scheduledFor,
        WorkoutType type,
        WorkoutIntensity intensity,
        decimal targetDistanceKm,
        int targetDurationSeconds,
        int targetPaceSecondsPerKm,
        string reason)
    {
        return PlannedSession(
            sortOrder,
            scheduledFor,
            type,
            intensity,
            targetDistanceKm,
            targetDurationSeconds,
            targetPaceSecondsPerKm,
            PlannedSessionStatus.Skipped,
            reason);
    }

    private static PlannedSession ScheduledSession(
        int sortOrder,
        DateTimeOffset scheduledFor,
        WorkoutType type,
        WorkoutIntensity intensity,
        decimal targetDistanceKm,
        int targetDurationSeconds,
        int targetPaceSecondsPerKm,
        string reason)
    {
        return PlannedSession(
            sortOrder,
            scheduledFor,
            type,
            intensity,
            targetDistanceKm,
            targetDurationSeconds,
            targetPaceSecondsPerKm,
            PlannedSessionStatus.Scheduled,
            reason);
    }

    private static PlannedSession PlannedSession(
        int sortOrder,
        DateTimeOffset scheduledFor,
        WorkoutType type,
        WorkoutIntensity intensity,
        decimal targetDistanceKm,
        int targetDurationSeconds,
        int targetPaceSecondsPerKm,
        PlannedSessionStatus status,
        string reason)
    {
        return new PlannedSession
        {
            SortOrder = sortOrder,
            ScheduledFor = scheduledFor,
            Type = type,
            Intensity = intensity,
            TargetDistanceKm = targetDistanceKm,
            TargetDurationSeconds = targetDurationSeconds,
            TargetPaceSecondsPerKm = targetPaceSecondsPerKm,
            Status = status,
            Reason = reason,
            Notes = null
        };
    }

    private static List<Workout> CreateActualWorkouts(
        int userId,
        IReadOnlyCollection<TrainingWeek> weeks)
    {
        var sessions = weeks
            .SelectMany(x => x.PlannedSessions)
            .ToList();

        return new List<Workout>
        {
            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 6),
                6.16m,
                2336,
                4,
                WorkoutType.Easy,
                WorkoutSource.Gpx,
                "Completed from GPX file: 2026-05-06_easy_obolon_6k.gpx. Actual distance and pace are close to the planned easy session."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 8),
                4.10m,
                1740,
                3,
                WorkoutType.Recovery,
                WorkoutSource.Manual,
                "Manual recovery run. Slightly longer than planned, still within easy intensity."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 13),
                7.26m,
                2576,
                8,
                WorkoutType.Quality,
                WorkoutSource.Gpx,
                "Completed from GPX file: 2026-05-13_quality_vdng_intervals_7k.gpx. Faster middle sections should be visible on the pace-colored route."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 15),
                5.40m,
                2110,
                4,
                WorkoutType.Easy,
                WorkoutSource.Manual,
                "Manual easy run after the quality workout."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 17),
                8.80m,
                3360,
                6,
                WorkoutType.Long,
                WorkoutSource.Manual,
                "Manual long run. Slightly shorter than planned but completed with controlled effort."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 20),
                10.46m,
                3859,
                6,
                WorkoutType.Long,
                WorkoutSource.Gpx,
                "Completed from GPX file: 2026-05-20_long_holosiivo_10k.gpx. Long run is slightly over target distance with mild late fade."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 24),
                6.30m,
                2400,
                4,
                WorkoutType.Easy,
                WorkoutSource.Manual,
                "Manual easy run used to preserve consistency after the skipped recovery session."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 27),
                8.06m,
                2870,
                8,
                WorkoutType.Quality,
                WorkoutSource.Gpx,
                "Completed from GPX file: 2026-05-27_progression_rusanivka_8k.gpx. The actual progression run closely matches the planned target."),

            WorkoutFor(
                userId,
                sessions,
                new DateOnly(2026, 5, 31),
                4.55m,
                1900,
                3,
                WorkoutType.Recovery,
                WorkoutSource.Manual,
                "Manual recovery run closing the May training block.")
        };
    }

    private static Workout WorkoutFor(
        int userId,
        IReadOnlyCollection<PlannedSession> sessions,
        DateOnly date,
        decimal distanceKm,
        int durationSeconds,
        int rpe,
        WorkoutType type,
        WorkoutSource source,
        string notes)
    {
        var plannedSession = sessions.Single(x =>
            x.ScheduledFor.HasValue &&
            DateOnly.FromDateTime(x.ScheduledFor.Value.UtcDateTime) == date &&
            x.Status == PlannedSessionStatus.Completed);

        return new Workout
        {
            UserId = userId,
            PlannedSession = plannedSession,
            StartedAt = plannedSession.ScheduledFor!.Value,
            DistanceKm = distanceKm,
            DurationSeconds = durationSeconds,
            Rpe = rpe,
            Type = type,
            Source = source,
            Notes = notes,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static DateTimeOffset Utc(
        int year,
        int month,
        int day,
        int hour,
        int minute)
    {
        return new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero);
    }
}