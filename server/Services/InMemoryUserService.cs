using CompanyPortal.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace CompanyPortal.Api.Services;

// Temporary store for practice. Replace with an EF Core-backed implementation later.
public class InMemoryUserService : IUserService
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly List<User> _users = new();

    public InMemoryUserService()
    {
        // Demo user for practice only - never hardcode credentials in a real app.
        var demoUser = new User
        {
            Id = 1,
            Email = "demo@company.test",
            DisplayName = "Demo User"
        };
        demoUser.PasswordHash = _passwordHasher.HashPassword(demoUser, "Demo1234!");
        _users.Add(demoUser);
    }

    public Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            return Task.FromResult<User?>(null);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        var isValid = result != PasswordVerificationResult.Failed;

        return Task.FromResult(isValid ? user : null);
    }
}
