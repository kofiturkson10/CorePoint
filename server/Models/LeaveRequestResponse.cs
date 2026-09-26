namespace CompanyPortal.Api.Models;

// What GET /api/leaverequests (the Admin/HR list) returns instead of the raw LeaveRequest
// entity, so the response can include the requester's email without adding it to the
// LeaveRequests table itself - EmployeeId there stays exactly as it is today.
public class LeaveRequestResponse
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }

    // Looked up from EmployeeId via IUserService. Null if that user can no longer be found
    // (e.g. a removed test user) - the list still renders, just without a name for that row.
    public string? RequesterEmail { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
