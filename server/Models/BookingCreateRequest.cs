using System.ComponentModel.DataAnnotations;

namespace CompanyPortal.Api.Models;

// What the client sends when booking a room.
// EmployeeId, CreatedAt etc. are set by the server, not the client.
public class BookingCreateRequest
{
    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}
