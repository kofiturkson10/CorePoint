namespace CompanyPortal.Api.Models;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }

    // The Id of the logged-in user (User.Id, from the auth cookie) who booked - same pattern
    // as LeaveRequest.EmployeeId, not a foreign key into the Employees catalog.
    public int EmployeeId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
