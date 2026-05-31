using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.RunnerProfiles;
using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Infrastructure.Persistence.Repositories;

public sealed class RunnerProfileRepository : IRunnerProfileRepository
{
    private readonly AppDbContext _dbContext;

    public RunnerProfileRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RunnerProfile?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RunnerProfiles
            .Include(profile => profile.AvailableDays)
            .FirstOrDefaultAsync(
                profile => profile.UserId == userId,
                cancellationToken);
    }

    public async Task<RunnerProfile> AddAsync(
        RunnerProfile profile,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RunnerProfiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return profile;
    }

    public async Task UpdateAsync(
        RunnerProfile profile,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RunnerProfiles.Update(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}