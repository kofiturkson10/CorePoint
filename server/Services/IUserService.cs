using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

public interface IUserService
{
    /// <summary>Returns the user if email and password are correct, otherwise null.</summary>
    Task<User?> ValidateCredentialsAsync(string email, string password);
}
