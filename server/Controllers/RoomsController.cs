using CompanyPortal.Api.Data;
using CompanyPortal.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public RoomsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<Room>>> GetAllAsync()
    {
        var rooms = await _dbContext.Rooms
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync();

        return Ok(rooms);
    }

    // The route Name lets CreateAsync build the Location header.
    [HttpGet("{id:int}", Name = "GetRoomById")]
    public async Task<ActionResult<Room>> GetByIdAsync(int id)
    {
        var room = await _dbContext.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room is null)
        {
            return NotFound();
        }

        return Ok(room);
    }

    // Only Admin manages the room catalog - unlike LeaveRequests, HR has no say here.
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<Room>> CreateAsync(RoomRequest request)
    {
        var room = new Room
        {
            Name = request.Name,
            Capacity = request.Capacity,
            Location = request.Location
        };

        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        return CreatedAtRoute("GetRoomById", new { id = room.Id }, room);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Room>> UpdateAsync(int id, RoomRequest request)
    {
        var room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null)
        {
            return NotFound();
        }

        room.Name = request.Name;
        room.Capacity = request.Capacity;
        room.Location = request.Location;

        await _dbContext.SaveChangesAsync();

        return Ok(room);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null)
        {
            return NotFound();
        }

        // Otherwise those bookings would be left pointing at a room that no longer exists.
        var hasActiveBookings = await _dbContext.Bookings
            .AnyAsync(b => b.RoomId == id && b.EndTime > DateTime.UtcNow);
        if (hasActiveBookings)
        {
            return Conflict(new { message = "This room has active or upcoming bookings and can't be deleted." });
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
