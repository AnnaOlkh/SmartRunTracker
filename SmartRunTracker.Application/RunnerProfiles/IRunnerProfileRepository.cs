using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.RunnerProfiles;

public interface IRunnerProfileRepository
{
    Task<RunnerProfile?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<RunnerProfile> AddAsync(
        RunnerProfile profile,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        RunnerProfile profile,
        CancellationToken cancellationToken = default);
}