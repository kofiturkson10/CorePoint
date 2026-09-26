using System.ComponentModel.DataAnnotations;

namespace CompanyPortal.Api.Models;

// What the client sends when applying for leave.
// EmployeeId, Status, CreatedAt etc. are set by the server, not the client.
public class LeaveRequestCreateRequest
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(1000)]
    public string? Reason { get; set; }
}
