using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Domain.Entities;

public class TrainingWeek
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RunningGoalId { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public int TargetWorkoutCount { get; set; }

    public AdjustmentMode AdjustmentMode { get; set; }

    public int TargetWeekDurationSeconds { get; set; }

    public TrainingWeekStatus Status { get; set; } = TrainingWeekStatus.Draft;

    public string? Explanation { get; set; }

    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;

    public RunningGoal RunningGoal { get; set; } = null!;

    public ICollection<PlannedSession> PlannedSessions { get; set; } = new List<PlannedSession>();
}