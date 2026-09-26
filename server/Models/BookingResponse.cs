namespace CompanyPortal.Api.Models;

// What GET /api/bookings (everyone's bookings) returns instead of the raw Booking entity,
// so the response can include the room's name and the requester's email without adding
// either to the Bookings table itself.
public class BookingResponse
{
    public int Id { get; set; }
    public int RoomId { get; set; }

    // Looked up from RoomId. Null if the room can no longer be found.
    public string? RoomName { get; set; }

    public int EmployeeId { get; set; }

    // Looked up from EmployeeId via IUserService. Null if that user can no longer be found.
    public string? RequesterEmail { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
