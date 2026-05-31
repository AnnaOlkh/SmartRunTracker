namespace SmartRunTracker.Application.RunnerProfiles;

public interface IRunnerProfileService
{
    Task<RunnerProfileDto> GetOrCreateAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<RunnerProfileDto> UpdateAsync(
        int userId,
        UpdateRunnerProfileRequest request,
        CancellationToken cancellationToken = default);
}