using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RunnerProfile> RunnerProfiles => Set<RunnerProfile>();
    public DbSet<RunningGoal> RunningGoals => Set<RunningGoal>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<TrainingWeek> TrainingWeeks => Set<TrainingWeek>();
    public DbSet<PlannedSession> PlannedSessions => Set<PlannedSession>();
    public DbSet<RunnerAvailableDay> RunnerAvailableDays => Set<RunnerAvailableDay>();
    public DbSet<WorkoutRoutePoint> WorkoutRoutePoints => Set<WorkoutRoutePoint>();
    public DbSet<WorkoutSplit> WorkoutSplits => Set<WorkoutSplit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureRunnerProfiles(modelBuilder);
        ConfigureRunningGoals(modelBuilder);
        ConfigureWorkouts(modelBuilder);
        ConfigureWorkoutRoutePoints(modelBuilder);
        ConfigureWorkoutSplits(modelBuilder);
        ConfigureTrainingWeeks(modelBuilder);
        ConfigurePlannedSessions(modelBuilder);
        ConfigureRunnerAvailableDays(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DisplayName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(255);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });
    }

    private static void ConfigureRunnerProfiles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RunnerProfile>(entity =>
        {
            entity.ToTable("runner_profiles");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PreferredWorkoutsPerWeek)
                .IsRequired();

            entity.Property(x => x.TrainingDayPreferenceMode)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt);

            entity.HasOne(x => x.User)
                .WithOne(x => x.RunnerProfile)
                .HasForeignKey<RunnerProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId)
                .IsUnique();
        });
    }

    private static void ConfigureRunningGoals(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RunningGoal>(entity =>
        {
            entity.ToTable("running_goals");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TargetDistanceKm)
                .HasPrecision(5, 2)
                .IsRequired();

            entity.Property(x => x.TargetPaceSecondsPerKm)
                .IsRequired();

            entity.Property(x => x.GoalDate);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.RunningGoals)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWorkouts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("workouts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartedAt)
                .IsRequired();

            entity.Property(x => x.DistanceKm)
                .HasPrecision(6, 2)
                .IsRequired();

            entity.Property(x => x.DurationSeconds)
                .IsRequired();

            entity.Property(x => x.Rpe)
                .IsRequired();

            entity.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Source)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Workouts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.PlannedSession)
                .WithOne(x => x.CompletedWorkout)
                .HasForeignKey<Workout>(x => x.PlannedSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.StartedAt);
        });
    }
    private static void ConfigureWorkoutRoutePoints(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutRoutePoint>(entity =>
        {
            entity.ToTable("workout_route_points");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Order)
                .IsRequired();

            entity.Property(x => x.Latitude)
                .HasPrecision(9, 6)
                .IsRequired();

            entity.Property(x => x.Longitude)
                .HasPrecision(9, 6)
                .IsRequired();

            entity.Property(x => x.ElevationMeters)
                .HasPrecision(8, 2);

            entity.Property(x => x.RecordedAt);

            entity.Property(x => x.DistanceFromStartMeters)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(x => x.SecondsFromStart);

            entity.Property(x => x.PaceSecondsPerKm);

            entity.HasOne(x => x.Workout)
                .WithMany(x => x.RoutePoints)
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.WorkoutId);

            entity.HasIndex(x => new { x.WorkoutId, x.Order })
                .IsUnique();
        });
    }
    private static void ConfigureWorkoutSplits(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutSplit>(entity =>
        {
            entity.ToTable("workout_splits");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SplitNumber)
                .IsRequired();

            entity.Property(x => x.DistanceKm)
                .HasPrecision(6, 3)
                .IsRequired();

            entity.Property(x => x.DurationSeconds)
                .IsRequired();

            entity.Property(x => x.AveragePaceSecondsPerKm)
                .IsRequired();

            entity.Property(x => x.StartedAt);

            entity.Property(x => x.EndedAt);

            entity.HasOne(x => x.Workout)
                .WithMany(x => x.Splits)
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.WorkoutId);

            entity.HasIndex(x => new { x.WorkoutId, x.SplitNumber })
                .IsUnique();
        });
    }
    private static void ConfigureTrainingWeeks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrainingWeek>(entity =>
        {
            entity.ToTable("training_weeks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WeekStartDate)
                .IsRequired();

            entity.Property(x => x.TargetWorkoutCount)
                .IsRequired();

            entity.Property(x => x.AdjustmentMode)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.TargetWeekDurationSeconds)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Explanation)
                .HasMaxLength(2000);

            entity.Property(x => x.GeneratedAt)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.TrainingWeeks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RunningGoal)
                .WithMany(x => x.TrainingWeeks)
                .HasForeignKey(x => x.RunningGoalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.UserId, x.WeekStartDate })
                .IsUnique();
        });
    }

    private static void ConfigurePlannedSessions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlannedSession>(entity =>
        {
            entity.ToTable("planned_sessions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Intensity)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.TargetDurationSeconds)
                .IsRequired();

            entity.Property(x => x.TargetDistanceKm)
                .HasPrecision(6, 2);

            entity.Property(x => x.TargetPaceSecondsPerKm);

            entity.Property(x => x.ScheduledFor);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.Property(x => x.Reason)
                .HasMaxLength(1000);

            entity.Property(x => x.SortOrder)
                .IsRequired();

            entity.HasOne(x => x.TrainingWeek)
                .WithMany(x => x.PlannedSessions)
                .HasForeignKey(x => x.TrainingWeekId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.TrainingWeekId);
            entity.HasIndex(x => x.ScheduledFor);
        });
    }
    private static void ConfigureRunnerAvailableDays(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RunnerAvailableDay>(entity =>
        {
            entity.ToTable("runner_available_days");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Day)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasOne(x => x.RunnerProfile)
                .WithMany(x => x.AvailableDays)
                .HasForeignKey(x => x.RunnerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.RunnerProfileId, x.Day })
                .IsUnique();
        });
    }
}