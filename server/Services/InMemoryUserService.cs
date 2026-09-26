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
        // Demo users for practice only - never hardcode credentials in a real app.
        // One user per role, so all three access levels can be tested locally.
        AddDemoUser(1, "admin@company.test", "Admin User", UserRole.Admin);
        AddDemoUser(2, "hr@company.test", "HR User", UserRole.HR);
        AddDemoUser(3, "demo@company.test", "Demo User", UserRole.Employee);
    }

    private void AddDemoUser(int id, string email, string displayName, UserRole role)
    {
        var user = new User
        {
            Id = id,
            Email = email,
            DisplayName = displayName,
            Role = role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, "Demo1234!");
        _users.Add(user);
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

    public Task<User?> FindByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }
}
