using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

public interface IUserService
{
    /// <summary>Returns the user if email and password are correct, otherwise null.</summary>
    Task<User?> ValidateCredentialsAsync(string email, string password);

    /// <summary>Returns the user with this id, or null if no such user exists.</summary>
    Task<User?> FindByIdAsync(int id);
}
