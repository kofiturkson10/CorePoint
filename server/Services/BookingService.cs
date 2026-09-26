using CompanyPortal.Api.Data;
using CompanyPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;

    public BookingService(AppDbContext dbContext, IUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<BookingCreateResult> CreateAsync(int employeeId, BookingCreateRequest request)
    {
        if (request.StartTime >= request.EndTime)
        {
            return new BookingCreateResult(BookingCreateOutcome.InvalidTimeRange, null);
        }

        var roomExists = await _dbContext.Rooms.AnyAsync(r => r.Id == request.RoomId);
        if (!roomExists)
        {
            return new BookingCreateResult(BookingCreateOutcome.RoomNotFound, null);
        }

        var startTime = ToUtc(request.StartTime);
        var endTime = ToUtc(request.EndTime);

        // Classic overlap check: two time ranges overlap when each starts before the
        // other one ends.
        var conflictingBooking = await _dbContext.Bookings
            .Where(b => b.RoomId == request.RoomId && b.StartTime < endTime && b.EndTime > startTime)
            .FirstOrDefaultAsync();

        if (conflictingBooking is not null)
        {
            return new BookingCreateResult(BookingCreateOutcome.Overlapping, null, conflictingBooking);
        }

        var booking = new Booking
        {
            RoomId = request.RoomId,
            EmployeeId = employeeId,
            StartTime = startTime,
            EndTime = endTime,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();

        return new BookingCreateResult(BookingCreateOutcome.Success, booking);
    }

    public async Task<List<BookingResponse>> GetAllAsync()
    {
        var bookings = await _dbContext.Bookings
            .AsNoTracking()
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        var rooms = await _dbContext.Rooms.AsNoTracking().ToDictionaryAsync(r => r.Id);

        var responses = new List<BookingResponse>(bookings.Count);
        foreach (var booking in bookings)
        {
            var requester = await _userService.FindByIdAsync(booking.EmployeeId);
            rooms.TryGetValue(booking.RoomId, out var room);

            responses.Add(new BookingResponse
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomName = room?.Name,
                EmployeeId = booking.EmployeeId,
                RequesterEmail = requester?.Email,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                CreatedAt = booking.CreatedAt
            });
        }

        return responses;
    }

    public async Task<List<Booking>> GetMineAsync(int employeeId)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.EmployeeId == employeeId)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<BookingCancelResult> CancelAsync(int id, int currentUserId, bool currentUserCanManageAnyBooking)
    {
        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
        {
            return new BookingCancelResult(BookingCancelOutcome.NotFound);
        }

        if (booking.EmployeeId != currentUserId && !currentUserCanManageAnyBooking)
        {
            return new BookingCancelResult(BookingCancelOutcome.Forbidden);
        }

        _dbContext.Bookings.Remove(booking);
        await _dbContext.SaveChangesAsync();

        return new BookingCancelResult(BookingCancelOutcome.Success);
    }

    // A date without a time zone ("2026-09-21T10:00:00") is treated as UTC, not as server-local time
    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
