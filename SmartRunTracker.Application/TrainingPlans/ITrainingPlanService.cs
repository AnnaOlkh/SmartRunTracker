namespace SmartRunTracker.Application.TrainingPlans;

public interface ITrainingPlanService
{
    Task<TrainingWeekDto> GenerateWeekAsync(
        int userId,
        GenerateTrainingWeekRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrainingWeekDto>> GetWeeksAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<TrainingWeekDto?> GetWeekByIdAsync(
        int userId,
        int trainingWeekId,
        CancellationToken cancellationToken = default);

    Task<PlannedSessionDto?> ScheduleSessionAsync(
        int userId,
        int plannedSessionId,
        SchedulePlannedSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<PlannedSessionDto?> SkipSessionAsync(
        int userId,
        int plannedSessionId,
        CancellationToken cancellationToken = default);
}