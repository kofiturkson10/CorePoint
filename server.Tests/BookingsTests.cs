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

public class BookingsTests : IClassFixture<TestingWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public BookingsTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task CreateBooking_AsLoggedInUser_ReturnsCreated()
    {
        await LoginAsync("admin@company.test");
        var roomId = await CreateRoomAsync();

        await LoginAsync("demo@company.test");
        var response = await _client.PostAsJsonAsync("/api/bookings", NewBooking(roomId, DateTime.UtcNow.AddDays(1)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_OverlappingSameRoom_ReturnsConflict()
    {
        await LoginAsync("admin@company.test");
        var roomId = await CreateRoomAsync();

        var start = DateTime.UtcNow.AddDays(2);
        var firstResponse = await _client.PostAsJsonAsync("/api/bookings", NewBooking(roomId, start));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Overlaps: starts 30 minutes into the first booking's hour-long slot
        var overlappingResponse = await _client.PostAsJsonAsync("/api/bookings", new
        {
            RoomId = roomId,
            StartTime = start.AddMinutes(30),
            EndTime = start.AddMinutes(90)
        });

        Assert.Equal(HttpStatusCode.Conflict, overlappingResponse.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_DifferentTimeSameRoom_Succeeds()
    {
        await LoginAsync("admin@company.test");
        var roomId = await CreateRoomAsync();

        var start = DateTime.UtcNow.AddDays(3);
        var firstResponse = await _client.PostAsJsonAsync("/api/bookings", NewBooking(roomId, start));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Starts two hours after the first booking's hour-long slot ends - no overlap
        var laterResponse = await _client.PostAsJsonAsync("/api/bookings", NewBooking(roomId, start.AddHours(2)));

        Assert.Equal(HttpStatusCode.Created, laterResponse.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_SameTimeDifferentRoom_Succeeds()
    {
        await LoginAsync("admin@company.test");
        var firstRoomId = await CreateRoomAsync();
        var secondRoomId = await CreateRoomAsync();

        var start = DateTime.UtcNow.AddDays(4);
        var firstResponse = await _client.PostAsJsonAsync("/api/bookings", NewBooking(firstRoomId, start));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync("/api/bookings", NewBooking(secondRoomId, start));

        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Cancel_OwnBooking_Succeeds()
    {
        await LoginAsync("admin@company.test");
        var roomId = await CreateRoomAsync();

        await LoginAsync("demo@company.test");
        var bookingId = await CreateBookingAsync(roomId, DateTime.UtcNow.AddDays(5));

        var response = await _client.DeleteAsync($"/api/bookings/{bookingId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_SomeoneElsesBooking_AsEmployee_ReturnsForbidden()
    {
        await LoginAsync("admin@company.test");
        var roomId = await CreateRoomAsync();
        var adminBookingId = await CreateBookingAsync(roomId, DateTime.UtcNow.AddDays(6));

        await LoginAsync("demo@company.test");
        var response = await _client.DeleteAsync($"/api/bookings/{adminBookingId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_AnyBooking_AsAdmin_Succeeds()
    {
        await LoginAsync("admin@company.test");
        var roomId = await CreateRoomAsync();

        await LoginAsync("demo@company.test");
        var bookingId = await CreateBookingAsync(roomId, DateTime.UtcNow.AddDays(7));

        await LoginAsync("admin@company.test");
        var response = await _client.DeleteAsync($"/api/bookings/{bookingId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<int> CreateRoomAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/rooms", new
        {
            Name = $"Room {Guid.NewGuid()}",
            Capacity = 8,
            Location = "Floor 2"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Room>(JsonOptions);
        return created!.Id;
    }

    private async Task<int> CreateBookingAsync(int roomId, DateTime start)
    {
        var response = await _client.PostAsJsonAsync("/api/bookings", NewBooking(roomId, start));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Booking>(JsonOptions);
        return created!.Id;
    }

    private static object NewBooking(int roomId, DateTime start)
    {
        return new { RoomId = roomId, StartTime = start, EndTime = start.AddHours(1) };
    }

    private async Task LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Demo1234!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
