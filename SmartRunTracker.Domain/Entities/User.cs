namespace SmartRunTracker.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public RunnerProfile? RunnerProfile { get; set; }

    public ICollection<RunningGoal> RunningGoals { get; set; } = new List<RunningGoal>();

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();

    public ICollection<TrainingWeek> TrainingWeeks { get; set; } = new List<TrainingWeek>();
}