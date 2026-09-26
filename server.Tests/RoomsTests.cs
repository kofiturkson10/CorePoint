using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CompanyPortal.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyPortal.Api.Tests;

public class RoomsTests : IClassFixture<TestingWebApplicationFactory>
{
    // The API returns camelCase JSON, but Room's C# properties are PascalCase - case-insensitive
    // matching is needed to deserialize responses back into it.
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public RoomsTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task CreateRoom_AsAdmin_ReturnsCreated()
    {
        await LoginAsync("admin@company.test");

        var response = await _client.PostAsJsonAsync("/api/rooms", NewRoom());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateRoom_AsEmployee_ReturnsForbidden()
    {
        await LoginAsync("demo@company.test");

        var response = await _client.PostAsJsonAsync("/api/rooms", NewRoom());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRoom_WithActiveBooking_ReturnsConflict()
    {
        await LoginAsync("admin@company.test");
        var roomId = await CreateRoomAsync();

        var bookingResponse = await _client.PostAsJsonAsync("/api/bookings", new
        {
            RoomId = roomId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, bookingResponse.StatusCode);

        var response = await _client.DeleteAsync($"/api/rooms/{roomId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<int> CreateRoomAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/rooms", NewRoom());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Room>(JsonOptions);
        return created!.Id;
    }

    private static object NewRoom()
    {
        return new
        {
            // Unique name every run, so this test doesn't collide with rooms from other tests.
            Name = $"Room {Guid.NewGuid()}",
            Capacity = 8,
            Location = "Floor 2"
        };
    }

    private async Task LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Demo1234!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
