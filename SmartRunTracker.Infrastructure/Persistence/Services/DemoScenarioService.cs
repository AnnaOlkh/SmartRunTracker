using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Infrastructure.Persistence.Services;

public sealed class DemoScenarioService : IDemoScenarioService
{
    private static readonly DateOnly ScenarioWeekStartDate = new(2026, 6, 1);

    private readonly AppDbContext _dbContext;

    public DemoScenarioService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DemoScenarioResultDto> SeedMaintainScenarioAsync(
        CancellationToken cancellationToken = default)
    {
        await ResetDemoDataAsync(cancellationToken);

        await SeedBaseAsync(
            preferredWorkoutsPerWeek: 4,
            availableDays:
            [
                TrainingDay.Monday,
                TrainingDay.Wednesday,
                TrainingDay.Friday,
                TrainingDay.Sunday
            ],
            cancellationToken);

        AddWorkout(new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero), 5.00m, 1900, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero), 6.00m, 2300, 6, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 29, 10, 0, 0, TimeSpan.Zero), 5.00m, 1800, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 31, 10, 0, 0, TimeSpan.Zero), 8.00m, 3400, 6, WorkoutType.Long);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoScenarioResultDto(
            "Maintain",
            ScenarioWeekStartDate,
            "Seeded 4 normal workouts in the last week and no previous-week baseline. Expected result: Maintain.");
    }

    public async Task<DemoScenarioResultDto> SeedProgressScenarioAsync(
        CancellationToken cancellationToken = default)
    {
        await ResetDemoDataAsync(cancellationToken);

        await SeedBaseAsync(
            preferredWorkoutsPerWeek: 4,
            availableDays:
            [
                TrainingDay.Monday,
                TrainingDay.Wednesday,
                TrainingDay.Friday,
                TrainingDay.Sunday
            ],
            cancellationToken);

        AddWorkout(new DateTimeOffset(2026, 5, 18, 10, 0, 0, TimeSpan.Zero), 4.50m, 1800, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 20, 10, 0, 0, TimeSpan.Zero), 5.50m, 2200, 6, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 23, 10, 0, 0, TimeSpan.Zero), 7.00m, 3000, 6, WorkoutType.Long);

        AddWorkout(new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero), 4.80m, 1900, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero), 5.80m, 2300, 6, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 30, 10, 0, 0, TimeSpan.Zero), 7.40m, 3200, 6, WorkoutType.Long);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoScenarioResultDto(
            "Progress",
            ScenarioWeekStartDate,
            "Seeded two stable weeks with controlled RPE and no sharp volume increase. Expected result: Progress.");
    }

    public async Task<DemoScenarioResultDto> SeedDeloadScenarioAsync(
        CancellationToken cancellationToken = default)
    {
        await ResetDemoDataAsync(cancellationToken);

        await SeedBaseAsync(
            preferredWorkoutsPerWeek: 4,
            availableDays:
            [
                TrainingDay.Monday,
                TrainingDay.Wednesday,
                TrainingDay.Friday,
                TrainingDay.Sunday
            ],
            cancellationToken);

        AddWorkout(new DateTimeOffset(2026, 5, 18, 10, 0, 0, TimeSpan.Zero), 4.50m, 1800, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 20, 10, 0, 0, TimeSpan.Zero), 5.50m, 2200, 6, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 23, 10, 0, 0, TimeSpan.Zero), 7.00m, 3000, 6, WorkoutType.Long);

        AddWorkout(new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero), 6.00m, 2600, 8, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero), 6.50m, 2800, 9, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 30, 10, 0, 0, TimeSpan.Zero), 9.00m, 3900, 8, WorkoutType.Long);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoScenarioResultDto(
            "Deload",
            ScenarioWeekStartDate,
            "Seeded high-RPE week with two quality workouts. Expected result: Deload.");
    }

    public async Task<DemoScenarioResultDto> SeedSkippedSessionsScenarioAsync(
        CancellationToken cancellationToken = default)
    {
        await ResetDemoDataAsync(cancellationToken);

        var goal = await SeedBaseAsync(
            preferredWorkoutsPerWeek: 4,
            availableDays:
            [
                TrainingDay.Monday,
                TrainingDay.Wednesday,
                TrainingDay.Friday,
                TrainingDay.Sunday
            ],
            cancellationToken);

        var previousGeneratedWeek = new TrainingWeek
        {
            UserId = DemoUser.Id,
            RunningGoalId = goal.Id,
            WeekStartDate = new DateOnly(2026, 5, 25),
            TargetWorkoutCount = 4,
            AdjustmentMode = AdjustmentMode.Maintain,
            TargetWeekDurationSeconds = 9000,
            Status = TrainingWeekStatus.Completed,
            Explanation = "Demo previous week with skipped planned sessions.",
            GeneratedAt = new DateTimeOffset(2026, 5, 24, 10, 0, 0, TimeSpan.Zero),
            PlannedSessions =
            [
                CreatePlannedSession(WorkoutType.Easy, WorkoutIntensity.Low, 1800, new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero), PlannedSessionStatus.Completed, 1),
                CreatePlannedSession(WorkoutType.Quality, WorkoutIntensity.Moderate, 2400, new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero), PlannedSessionStatus.Skipped, 2),
                CreatePlannedSession(WorkoutType.Easy, WorkoutIntensity.Low, 1800, new DateTimeOffset(2026, 5, 29, 10, 0, 0, TimeSpan.Zero), PlannedSessionStatus.Skipped, 3),
                CreatePlannedSession(WorkoutType.Long, WorkoutIntensity.Moderate, 3000, new DateTimeOffset(2026, 5, 31, 10, 0, 0, TimeSpan.Zero), PlannedSessionStatus.Skipped, 4)
            ]
        };

        _dbContext.TrainingWeeks.Add(previousGeneratedWeek);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var completedSessionId = previousGeneratedWeek.PlannedSessions
            .Single(session => session.Status == PlannedSessionStatus.Completed)
            .Id;

        AddWorkout(
            new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero),
            4.50m,
            1800,
            5,
            WorkoutType.Easy,
            completedSessionId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoScenarioResultDto(
            "Skipped sessions",
            ScenarioWeekStartDate,
            "Seeded 4 planned sessions where 3 were skipped. Expected result: Maintain with conservative return and no Quality session.");
    }

    public async Task<DemoScenarioResultDto> SeedLowConsistencyScenarioAsync(
        CancellationToken cancellationToken = default)
    {
        await ResetDemoDataAsync(cancellationToken);

        await SeedBaseAsync(
            preferredWorkoutsPerWeek: 4,
            availableDays:
            [
                TrainingDay.Monday,
                TrainingDay.Wednesday,
                TrainingDay.Friday,
                TrainingDay.Sunday
            ],
            cancellationToken);

        AddWorkout(new DateTimeOffset(2026, 5, 18, 10, 0, 0, TimeSpan.Zero), 5.00m, 2100, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 20, 10, 0, 0, TimeSpan.Zero), 6.00m, 2400, 6, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero), 5.00m, 2100, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 24, 10, 0, 0, TimeSpan.Zero), 8.00m, 3400, 6, WorkoutType.Long);

        AddWorkout(new DateTimeOffset(2026, 5, 28, 10, 0, 0, TimeSpan.Zero), 4.00m, 1800, 5, WorkoutType.Easy);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoScenarioResultDto(
            "Low consistency",
            ScenarioWeekStartDate,
            "Seeded a normal previous week and only one completed workout last week. Expected result: Maintain with conservative return.");
    }

    public async Task<DemoScenarioResultDto> SeedSixDaysScenarioAsync(
        CancellationToken cancellationToken = default)
    {
        await ResetDemoDataAsync(cancellationToken);

        await SeedBaseAsync(
            preferredWorkoutsPerWeek: 6,
            availableDays:
            [
                TrainingDay.Monday,
                TrainingDay.Tuesday,
                TrainingDay.Wednesday,
                TrainingDay.Thursday,
                TrainingDay.Friday,
                TrainingDay.Sunday
            ],
            cancellationToken);

        AddWorkout(new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero), 4.00m, 1800, 5, WorkoutType.Recovery);
        AddWorkout(new DateTimeOffset(2026, 5, 26, 10, 0, 0, TimeSpan.Zero), 5.00m, 2100, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero), 6.00m, 2400, 6, WorkoutType.Quality);
        AddWorkout(new DateTimeOffset(2026, 5, 28, 10, 0, 0, TimeSpan.Zero), 5.00m, 2100, 5, WorkoutType.Easy);
        AddWorkout(new DateTimeOffset(2026, 5, 29, 10, 0, 0, TimeSpan.Zero), 4.00m, 1800, 5, WorkoutType.Recovery);
        AddWorkout(new DateTimeOffset(2026, 5, 31, 10, 0, 0, TimeSpan.Zero), 8.00m, 3600, 6, WorkoutType.Long);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoScenarioResultDto(
            "Six days",
            ScenarioWeekStartDate,
            "Seeded profile with 6 preferred workouts and 6 available days. Expected result: 6 planned sessions.");
    }

    private async Task<RunningGoal> SeedBaseAsync(
        int preferredWorkoutsPerWeek,
        IReadOnlyList<TrainingDay> availableDays,
        CancellationToken cancellationToken)
    {
        await EnsureDemoUserAsync(cancellationToken);

        var profile = new RunnerProfile
        {
            UserId = DemoUser.Id,
            PreferredWorkoutsPerWeek = preferredWorkoutsPerWeek,
            TrainingDayPreferenceMode = TrainingDayPreferenceMode.SelectedDays,
            CreatedAt = DateTimeOffset.UtcNow,
            AvailableDays = availableDays
                .Distinct()
                .Select(day => new RunnerAvailableDay
                {
                    Day = day
                })
                .ToList()
        };

        var goal = new RunningGoal
        {
            UserId = DemoUser.Id,
            TargetDistanceKm = 10.00m,
            TargetPaceSecondsPerKm = 360,
            GoalDate = new DateOnly(2026, 8, 1),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.RunnerProfiles.Add(profile);
        _dbContext.RunningGoals.Add(goal);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return goal;
    }

    private async Task ResetDemoDataAsync(CancellationToken cancellationToken)
    {
        var demoWorkouts = await _dbContext.Workouts
            .Where(workout => workout.UserId == DemoUser.Id)
            .ToListAsync(cancellationToken);

        _dbContext.Workouts.RemoveRange(demoWorkouts);

        var demoTrainingWeeks = await _dbContext.TrainingWeeks
            .Include(week => week.PlannedSessions)
            .Where(week => week.UserId == DemoUser.Id)
            .ToListAsync(cancellationToken);

        _dbContext.TrainingWeeks.RemoveRange(demoTrainingWeeks);

        var demoGoals = await _dbContext.RunningGoals
            .Where(goal => goal.UserId == DemoUser.Id)
            .ToListAsync(cancellationToken);

        _dbContext.RunningGoals.RemoveRange(demoGoals);

        var demoProfiles = await _dbContext.RunnerProfiles
            .Include(profile => profile.AvailableDays)
            .Where(profile => profile.UserId == DemoUser.Id)
            .ToListAsync(cancellationToken);

        _dbContext.RunnerProfiles.RemoveRange(demoProfiles);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDemoUserAsync(CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(user => user.Id == DemoUser.Id, cancellationToken);

        if (userExists)
        {
            return;
        }

        _dbContext.Users.Add(new User
        {
            Id = DemoUser.Id,
            DisplayName = DemoUser.DisplayName,
            Email = DemoUser.Email,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AddWorkout(
        DateTimeOffset startedAt,
        decimal distanceKm,
        int durationSeconds,
        int rpe,
        WorkoutType type,
        int? plannedSessionId = null)
    {
        _dbContext.Workouts.Add(new Workout
        {
            UserId = DemoUser.Id,
            PlannedSessionId = plannedSessionId,
            StartedAt = startedAt,
            DistanceKm = distanceKm,
            DurationSeconds = durationSeconds,
            Rpe = rpe,
            Type = type,
            Source = WorkoutSource.Manual,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static PlannedSession CreatePlannedSession(
        WorkoutType type,
        WorkoutIntensity intensity,
        int targetDurationSeconds,
        DateTimeOffset scheduledFor,
        PlannedSessionStatus status,
        int sortOrder)
    {
        var targetPaceSecondsPerKm = type switch
        {
            WorkoutType.Recovery => 430,
            WorkoutType.Easy => 400,
            WorkoutType.Quality => 385,
            WorkoutType.Long => 410,
            _ => 400
        };

        return new PlannedSession
        {
            Type = type,
            Intensity = intensity,
            TargetDurationSeconds = targetDurationSeconds,
            TargetDistanceKm = Math.Round(
                targetDurationSeconds / (decimal)targetPaceSecondsPerKm,
                2,
                MidpointRounding.AwayFromZero),
            TargetPaceSecondsPerKm = targetPaceSecondsPerKm,
            ScheduledFor = scheduledFor,
            Status = status,
            Notes = "Demo planned session.",
            Reason = "Seeded for demo scenario.",
            SortOrder = sortOrder
        };
    }
}