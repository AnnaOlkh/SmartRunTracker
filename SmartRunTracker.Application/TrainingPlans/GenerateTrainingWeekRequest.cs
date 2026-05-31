namespace SmartRunTracker.Application.TrainingPlans;

public sealed record GenerateTrainingWeekRequest(
    DateOnly WeekStartDate
);