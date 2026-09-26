using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

public enum BookingCreateOutcome
{
    Success,
    InvalidTimeRange,
    RoomNotFound,
    Overlapping
}

// ConflictingBooking is set only when Outcome is Overlapping, so the controller can build a
// message that says which time the room is already booked for.
public record BookingCreateResult(BookingCreateOutcome Outcome, Booking? Booking, Booking? ConflictingBooking = null);

public enum BookingCancelOutcome
{
    Success,
    NotFound,
    Forbidden
}

public record BookingCancelResult(BookingCancelOutcome Outcome);

public interface IBookingService
{
    /// <summary>
    /// Creates a new booking. Fails if StartTime isn't before EndTime, the room doesn't exist,
    /// or the new time range overlaps an existing booking for the same room.
    /// </summary>
    Task<BookingCreateResult> CreateAsync(int employeeId, BookingCreateRequest request);

    /// <summary>Returns every booking, soonest first, with the room name and requester's email resolved in.</summary>
    Task<List<BookingResponse>> GetAllAsync();

    /// <summary>Returns the given employee's own bookings, soonest first.</summary>
    Task<List<Booking>> GetMineAsync(int employeeId);

    /// <summary>
    /// Cancels (deletes) a booking. Only the employee who made it, or someone who can manage
    /// any booking (Admin/HR), may cancel it.
    /// </summary>
    Task<BookingCancelResult> CancelAsync(int id, int currentUserId, bool currentUserCanManageAnyBooking);
}
