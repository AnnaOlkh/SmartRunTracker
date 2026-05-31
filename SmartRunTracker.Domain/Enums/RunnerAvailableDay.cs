using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Domain.Entities;

public class RunnerAvailableDay
{
    public int Id { get; set; }

    public int RunnerProfileId { get; set; }

    public TrainingDay Day { get; set; }

    public RunnerProfile RunnerProfile { get; set; } = null!;
}