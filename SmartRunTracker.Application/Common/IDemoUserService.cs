namespace SmartRunTracker.Application.Common;

public interface IDemoUserService
{
    Task EnsureDemoUserAsync(CancellationToken cancellationToken = default);
}