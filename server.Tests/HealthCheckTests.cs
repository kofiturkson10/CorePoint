using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyPortal.Api.Tests;

public class HealthCheckTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithHealthyStatus()
    {
        // TestingWebApplicationFactory gives the app a working in-memory Sqlite database,
        // so the database check should pass and the overall status should be Healthy.
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        Assert.Equal("Healthy", document.RootElement.GetProperty("status").GetString());

        var databaseCheck = document.RootElement.GetProperty("checks").EnumerateArray()
            .Single(check => check.GetProperty("name").GetString() == "database");
        Assert.Equal("Healthy", databaseCheck.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetHealth_DoesNotRequireLogin()
    {
        // No login cookie sent at all - Azure must be able to reach this anonymously.
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
