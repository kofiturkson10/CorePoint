using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyPortal.Api.Tests;

// WebApplicationFactory startar hela din API-app i minnet, ungefär som
// om den vore igång på riktigt, men utan att behöva köra "dotnet run"
// eller ha en webbläsare öppen. Varje testmetod nedan skickar ett
// riktigt HTTP-anrop mot den startade appen och kontrollerar svaret.
public class ApiTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiTests(TestingWebApplicationFactory factory)
    {
        // Auth-cookien sätts med Secure (skickas bara över HTTPS). TestServer kör
        // aldrig riktig TLS, men genom att låta klienten tro att den pratar https
        // skickar CookieContainer ändå med cookien på efterföljande anrop.
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Health_ReturnsOk_WithoutLogin()
    {
        // Ingen inloggning skickas med här - health-endpointen ska
        // vara nåbar oavsett, så att Azure kan övervaka appen.
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employees_WithoutLogin_ReturnsUnauthorized()
    {
        // Ingen inloggning skickas med här heller - då ska cookie-authen
        // du satt upp neka anropet med 401, inte t.ex. krascha med 500
        // eller (värre) råka släppa igenom med 200.
        var response = await _client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // POST /api/employees är en skriv-endpoint som bara Admin och HR får använda.
    // En inloggad Anställd ska nekas med 403 Forbidden, inte komma igenom.
    [Theory]
    [InlineData("admin@company.test", HttpStatusCode.Created)]
    [InlineData("hr@company.test", HttpStatusCode.Created)]
    [InlineData("demo@company.test", HttpStatusCode.Forbidden)]
    public async Task CreateEmployee_OnlyAllowedForAdminAndHrRoles(string email, HttpStatusCode expectedStatus)
    {
        await LoginAsync(email, "Demo1234!");

        var newEmployee = new
        {
            Name = "Test Testsson",
            Department = "IT",
            Role = "Developer",
            // Unik e-post varje körning, så testet inte krockar med tidigare skapade anställda.
            Email = $"{Guid.NewGuid()}@company.test"
        };

        var response = await _client.PostAsJsonAsync("/api/employees", newEmployee);

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    private async Task LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}