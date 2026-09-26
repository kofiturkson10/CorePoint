using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyPortal.Api.Tests;

// WebApplicationFactory startar hela din API-app i minnet, ungefär som
// om den vore igång på riktigt, men utan att behöva köra "dotnet run"
// eller ha en webbläsare öppen. Varje testmetod nedan skickar ett
// riktigt HTTP-anrop mot den startade appen och kontrollerar svaret.
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
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
}