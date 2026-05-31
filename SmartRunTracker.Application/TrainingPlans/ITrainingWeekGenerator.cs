namespace SmartRunTracker.Application.TrainingPlans;

public interface ITrainingWeekGenerator
{
    GeneratedTrainingWeek Generate(TrainingWeekGenerationInput input);
}