using System.ComponentModel.DataAnnotations;

namespace CompanyPortal.Api.Models;

// What the client sends when creating or updating an employee.
// Separate from Employee so the client can never set the Id.
public class EmployeeRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
}
