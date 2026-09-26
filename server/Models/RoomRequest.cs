using System.ComponentModel.DataAnnotations;

namespace CompanyPortal.Api.Models;

// What the client sends when creating or updating a room.
public class RoomRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Capacity { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }
}
