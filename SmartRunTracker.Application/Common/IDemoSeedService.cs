namespace SmartRunTracker.Application.Demo;

public interface IDemoSeedService
{
    Task<DemoSeedResult> SeedMay2026Async(
        int userId,
        CancellationToken cancellationToken = default);
}

public sealed record DemoSeedResult(
    int UserId,
    int GoalId,
    int WorkoutCount,
    int GpxWorkoutCount,
    DateOnly From,
    DateOnly To);