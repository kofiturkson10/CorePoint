using CompanyPortal.Api.Data;
using CompanyPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;

    public LeaveRequestService(AppDbContext dbContext, IUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<LeaveRequestCreateResult> CreateAsync(int employeeId, LeaveRequestCreateRequest request)
    {
        if (request.StartDate >= request.EndDate)
        {
            return new LeaveRequestCreateResult(LeaveRequestCreateOutcome.InvalidDateRange, null);
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employeeId,
            StartDate = ToUtc(request.StartDate),
            EndDate = ToUtc(request.EndDate),
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.LeaveRequests.Add(leaveRequest);
        await _dbContext.SaveChangesAsync();

        return new LeaveRequestCreateResult(LeaveRequestCreateOutcome.Success, leaveRequest);
    }

    public async Task<List<LeaveRequest>> GetMineAsync(int employeeId)
    {
        return await _dbContext.LeaveRequests
            .AsNoTracking()
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<LeaveRequestResponse>> GetAllAsync()
    {
        var leaveRequests = await _dbContext.LeaveRequests
            .AsNoTracking()
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();

        var responses = new List<LeaveRequestResponse>(leaveRequests.Count);
        foreach (var leaveRequest in leaveRequests)
        {
            // EmployeeId is really the applicant's User.Id (see LeaveRequest.cs) - resolved here
            // to an email for display. Left null if that user can no longer be found.
            var requester = await _userService.FindByIdAsync(leaveRequest.EmployeeId);

            responses.Add(new LeaveRequestResponse
            {
                Id = leaveRequest.Id,
                EmployeeId = leaveRequest.EmployeeId,
                RequesterEmail = requester?.Email,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status,
                CreatedAt = leaveRequest.CreatedAt,
                ReviewedByUserId = leaveRequest.ReviewedByUserId,
                ReviewedAt = leaveRequest.ReviewedAt
            });
        }

        return responses;
    }

    public Task<LeaveRequestReviewResult> ApproveAsync(int id, int reviewerUserId) =>
        ReviewAsync(id, reviewerUserId, LeaveRequestStatus.Approved);

    public Task<LeaveRequestReviewResult> RejectAsync(int id, int reviewerUserId) =>
        ReviewAsync(id, reviewerUserId, LeaveRequestStatus.Rejected);

    private async Task<LeaveRequestReviewResult> ReviewAsync(int id, int reviewerUserId, LeaveRequestStatus newStatus)
    {
        // Tracked on purpose: EF notices which properties we change and updates only those
        var leaveRequest = await _dbContext.LeaveRequests.FirstOrDefaultAsync(lr => lr.Id == id);
        if (leaveRequest is null)
        {
            return new LeaveRequestReviewResult(LeaveRequestReviewOutcome.NotFound, null);
        }

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            return new LeaveRequestReviewResult(LeaveRequestReviewOutcome.AlreadyReviewed, leaveRequest);
        }

        leaveRequest.Status = newStatus;
        leaveRequest.ReviewedByUserId = reviewerUserId;
        leaveRequest.ReviewedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new LeaveRequestReviewResult(LeaveRequestReviewOutcome.Success, leaveRequest);
    }

    // A date without a time zone ("2026-09-21T10:00:00") is treated as UTC, not as server-local time
    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
