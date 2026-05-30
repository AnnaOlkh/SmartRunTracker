namespace SmartRunTracker.Domain.Entities;

public class RunningGoal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal TargetDistanceKm { get; set; }

    public int TargetPaceSecondsPerKm { get; set; }

    public DateOnly? GoalDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;

    public ICollection<TrainingWeek> TrainingWeeks { get; set; } = new List<TrainingWeek>();
}