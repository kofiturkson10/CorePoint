using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CompanyPortal.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyPortal.Api.Tests;

public class LeaveRequestsTests : IClassFixture<TestingWebApplicationFactory>
{
    // The API returns camelCase JSON (e.g. "employeeId"), but LeaveRequest's C# properties are
    // PascalCase - case-insensitive matching is needed to deserialize responses back into it.
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public LeaveRequestsTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task CreateLeaveRequest_AsLoggedInEmployee_ReturnsCreated()
    {
        await LoginAsync("demo@company.test");

        var response = await _client.PostAsJsonAsync("/api/leaverequests", NewLeaveRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsEmployee_ReturnsForbidden()
    {
        await LoginAsync("demo@company.test");

        var response = await _client.GetAsync("/api/leaverequests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMine_OnlyReturnsOwnLeaveRequests()
    {
        await LoginAsync("admin@company.test");
        var adminRequestId = await CreateLeaveRequestAsync();

        await LoginAsync("demo@company.test");
        var demoRequestId = await CreateLeaveRequestAsync();

        var response = await _client.GetAsync("/api/leaverequests/mine");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var mine = await response.Content.ReadFromJsonAsync<List<LeaveRequest>>(JsonOptions);
        Assert.Contains(mine!, lr => lr.Id == demoRequestId);
        Assert.DoesNotContain(mine!, lr => lr.Id == adminRequestId);
    }

    [Fact]
    public async Task Approve_AsEmployee_ReturnsForbidden()
    {
        await LoginAsync("demo@company.test");
        var id = await CreateLeaveRequestAsync();

        var response = await _client.PutAsync($"/api/leaverequests/{id}/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Approve_AsAdmin_ReturnsOkAndSetsStatusApproved()
    {
        await LoginAsync("demo@company.test");
        var id = await CreateLeaveRequestAsync();

        await LoginAsync("admin@company.test");
        var response = await _client.PutAsync($"/api/leaverequests/{id}/approve", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var approved = await response.Content.ReadFromJsonAsync<LeaveRequest>(JsonOptions);
        Assert.Equal(LeaveRequestStatus.Approved, approved!.Status);
    }

    [Fact]
    public async Task GetAll_AsAdmin_IncludesRequesterEmail()
    {
        await LoginAsync("demo@company.test");
        var id = await CreateLeaveRequestAsync();

        await LoginAsync("admin@company.test");
        var response = await _client.GetAsync("/api/leaverequests");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var all = await response.Content.ReadFromJsonAsync<List<LeaveRequestResponse>>(JsonOptions);
        var created = Assert.Single(all!, lr => lr.Id == id);
        Assert.Equal("demo@company.test", created.RequesterEmail);
    }

    // Approving an already-handled request must fail cleanly (409), not throw or double-approve.
    [Fact]
    public async Task Approve_AlreadyApprovedRequest_ReturnsConflictInsteadOfCrashing()
    {
        await LoginAsync("demo@company.test");
        var id = await CreateLeaveRequestAsync();

        await LoginAsync("admin@company.test");
        var firstApprove = await _client.PutAsync($"/api/leaverequests/{id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, firstApprove.StatusCode);

        var secondApprove = await _client.PutAsync($"/api/leaverequests/{id}/approve", null);

        Assert.Equal(HttpStatusCode.Conflict, secondApprove.StatusCode);
    }

    private async Task<int> CreateLeaveRequestAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/leaverequests", NewLeaveRequest());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<LeaveRequest>(JsonOptions);
        return created!.Id;
    }

    private static object NewLeaveRequest()
    {
        return new
        {
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            Reason = "Semester"
        };
    }

    private async Task LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Demo1234!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
