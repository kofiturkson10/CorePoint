using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

public enum LeaveRequestCreateOutcome
{
    Success,
    InvalidDateRange
}

public record LeaveRequestCreateResult(LeaveRequestCreateOutcome Outcome, LeaveRequest? LeaveRequest);

public enum LeaveRequestReviewOutcome
{
    Success,
    NotFound,
    AlreadyReviewed
}

public record LeaveRequestReviewResult(LeaveRequestReviewOutcome Outcome, LeaveRequest? LeaveRequest);

public interface ILeaveRequestService
{
    /// <summary>Creates a new, pending leave request for the given employee.</summary>
    Task<LeaveRequestCreateResult> CreateAsync(int employeeId, LeaveRequestCreateRequest request);

    /// <summary>Returns the given employee's own leave requests, newest first.</summary>
    Task<List<LeaveRequest>> GetMineAsync(int employeeId);

    /// <summary>Returns every leave request, newest first.</summary>
    Task<List<LeaveRequest>> GetAllAsync();

    /// <summary>Approves a pending leave request. Fails if it doesn't exist or was already reviewed.</summary>
    Task<LeaveRequestReviewResult> ApproveAsync(int id, int reviewerUserId);

    /// <summary>Rejects a pending leave request. Fails if it doesn't exist or was already reviewed.</summary>
    Task<LeaveRequestReviewResult> RejectAsync(int id, int reviewerUserId);
}
