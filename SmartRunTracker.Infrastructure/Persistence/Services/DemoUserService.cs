using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Infrastructure.Persistence.Services;

public sealed class DemoUserService : IDemoUserService
{
    private readonly AppDbContext _dbContext;

    public DemoUserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureDemoUserAsync(
        CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Users
            .AnyAsync(user => user.Id == DemoUser.Id, cancellationToken);

        if (exists)
        {
            return;
        }

        var user = new User
        {
            Id = DemoUser.Id,
            DisplayName = DemoUser.DisplayName,
            Email = DemoUser.Email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}