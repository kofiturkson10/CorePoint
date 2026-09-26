using System.Security.Claims;
using CompanyPortal.Api.Models;
using CompanyPortal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompanyPortal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestsController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> CreateAsync(LeaveRequestCreateRequest request)
    {
        var result = await _leaveRequestService.CreateAsync(GetCurrentUserId(), request);
        if (result.Outcome == LeaveRequestCreateOutcome.InvalidDateRange)
        {
            return BadRequest(new { message = "StartDate must be before EndDate." });
        }

        var leaveRequest = result.LeaveRequest!;
        return Created($"/api/leaverequests/{leaveRequest.Id}", leaveRequest);
    }

    // Every logged-in user may see their own leave requests.
    [HttpGet("mine")]
    public async Task<ActionResult<List<LeaveRequest>>> GetMineAsync()
    {
        var leaveRequests = await _leaveRequestService.GetMineAsync(GetCurrentUserId());
        return Ok(leaveRequests);
    }

    // Only Admin/HR may see everyone's leave requests, so they can manage them.
    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.HR))]
    [HttpGet]
    public async Task<ActionResult<List<LeaveRequest>>> GetAllAsync()
    {
        var leaveRequests = await _leaveRequestService.GetAllAsync();
        return Ok(leaveRequests);
    }

    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.HR))]
    [HttpPut("{id:int}/approve")]
    public async Task<ActionResult<LeaveRequest>> ApproveAsync(int id)
    {
        var result = await _leaveRequestService.ApproveAsync(id, GetCurrentUserId());
        return ToActionResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.HR))]
    [HttpPut("{id:int}/reject")]
    public async Task<ActionResult<LeaveRequest>> RejectAsync(int id)
    {
        var result = await _leaveRequestService.RejectAsync(id, GetCurrentUserId());
        return ToActionResult(result);
    }

    private ActionResult<LeaveRequest> ToActionResult(LeaveRequestReviewResult result)
    {
        return result.Outcome switch
        {
            LeaveRequestReviewOutcome.NotFound => NotFound(),
            LeaveRequestReviewOutcome.AlreadyReviewed => Conflict(new
            {
                message = $"This leave request has already been {result.LeaveRequest!.Status}."
            }),
            _ => Ok(result.LeaveRequest)
        };
    }

    // Who applies/reviews comes from the login cookie, not from the request, so it can't be faked
    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
