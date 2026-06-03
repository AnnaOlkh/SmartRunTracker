using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application.Auth;

public interface IPasswordHashService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string passwordHash, string password);
}