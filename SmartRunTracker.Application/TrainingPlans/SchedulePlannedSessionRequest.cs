namespace SmartRunTracker.Application.TrainingPlans;

public sealed record SchedulePlannedSessionRequest(
    DateTimeOffset ScheduledFor
);