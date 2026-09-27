using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyPortal.Api.Tests;

public class DocumentDownloadTests : IClassFixture<TestingWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public DocumentDownloadTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetDownloadUrl_AsLoggedInUser_ReturnsDownloadUrl()
    {
        await LoginAsync("admin@company.test");
        var id = await UploadDocumentAsync($"{Guid.NewGuid()}.txt");

        var response = await _client.GetAsync($"/api/documents/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DownloadUrlResponse>(JsonOptions);
        Assert.False(string.IsNullOrWhiteSpace(body!.DownloadUrl));
        Assert.True(Uri.TryCreate(body.DownloadUrl, UriKind.Absolute, out _));
    }

    [Fact]
    public async Task GetDownloadUrl_WithoutLogin_ReturnsUnauthorized()
    {
        // No login cookie sent - getting the (short-lived) link still requires login,
        // even though the link itself needs no further authentication once issued.
        var response = await _client.GetAsync($"/api/documents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDownloadUrl_ForMissingDocument_ReturnsNotFound()
    {
        await LoginAsync("admin@company.test");

        var response = await _client.GetAsync($"/api/documents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> UploadDocumentAsync(string fileName)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", fileName);

        var response = await _client.PostAsync("/api/documents", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Demo1234!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private class DownloadUrlResponse
    {
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
