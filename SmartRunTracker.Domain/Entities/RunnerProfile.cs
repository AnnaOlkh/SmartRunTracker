namespace SmartRunTracker.Domain.Entities;

public class RunnerProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PreferredWorkoutsPerWeek { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}