namespace SmartRunTracker.Application.Common;

public interface ICurrentUserService
{
    int UserId { get; }

    bool IsAuthenticated { get; }
}