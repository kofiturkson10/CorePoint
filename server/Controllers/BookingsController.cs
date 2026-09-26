using System.Security.Claims;
using CompanyPortal.Api.Models;
using CompanyPortal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompanyPortal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<ActionResult<Booking>> CreateAsync(BookingCreateRequest request)
    {
        var result = await _bookingService.CreateAsync(GetCurrentUserId(), request);

        switch (result.Outcome)
        {
            case BookingCreateOutcome.InvalidTimeRange:
                return BadRequest(new { message = "StartTime must be before EndTime." });
            case BookingCreateOutcome.RoomNotFound:
                return NotFound(new { message = "No room with this id exists." });
            case BookingCreateOutcome.Overlapping:
                var conflicting = result.ConflictingBooking!;
                return Conflict(new
                {
                    message = $"The room is already booked from {conflicting.StartTime:yyyy-MM-dd HH:mm} " +
                               $"to {conflicting.EndTime:yyyy-MM-dd HH:mm} (UTC)."
                });
        }

        var booking = result.Booking!;
        return Created($"/api/bookings/{booking.Id}", booking);
    }

    // Everyone can see the full schedule, so they know what's free - no sensitive information here.
    [HttpGet]
    public async Task<ActionResult<List<BookingResponse>>> GetAllAsync()
    {
        var bookings = await _bookingService.GetAllAsync();
        return Ok(bookings);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<Booking>>> GetMineAsync()
    {
        var bookings = await _bookingService.GetMineAsync(GetCurrentUserId());
        return Ok(bookings);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelAsync(int id)
    {
        var canManageAnyBooking = User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.HR));
        var result = await _bookingService.CancelAsync(id, GetCurrentUserId(), canManageAnyBooking);

        return result.Outcome switch
        {
            BookingCancelOutcome.NotFound => NotFound(),
            BookingCancelOutcome.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

    // Who books/cancels comes from the login cookie, not from the request, so it can't be faked
    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
