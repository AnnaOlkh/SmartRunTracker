using Microsoft.AspNetCore.Identity;
using SmartRunTracker.Application.Auth;
using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Infrastructure.Auth;

public sealed class PasswordHashService : IPasswordHashService
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordHashService(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}