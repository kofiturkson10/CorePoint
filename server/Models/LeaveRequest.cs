namespace CompanyPortal.Api.Models;

public class LeaveRequest
{
    public int Id { get; set; }

    // The Id of the logged-in user (User.Id, from the auth cookie) who applied - not a foreign
    // key into the Employees catalog, since auth users and employee records aren't linked yet.
    public int EmployeeId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public DateTime CreatedAt { get; set; }

    // Both set together, only once the request moves out of Pending.
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
