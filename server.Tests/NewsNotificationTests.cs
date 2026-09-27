using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CompanyPortal.Api.Models;
using CompanyPortal.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CompanyPortal.Api.Tests;

public class NewsNotificationTests : IClassFixture<TestingWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly TestingWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public NewsNotificationTests(TestingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task CreateNews_ReturnsCreated_AndQueuesNotificationWithCorrectContent()
    {
        await LoginAsync("admin@company.test");

        var title = $"Test news {Guid.NewGuid()}";
        var response = await _client.PostAsJsonAsync("/api/news", new { Title = title, Content = "Some content" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<News>(JsonOptions);

        // The same fake queue instance the app used, resolved straight from the host's
        // service provider (it's a singleton, so this is exactly what NewsController called).
        var fakeQueue = (FakeNewsNotificationQueue)_factory.Services.GetRequiredService<INewsNotificationQueue>();
        var message = Assert.Single(fakeQueue.SentMessages, m => m.NewsId == created!.Id);
        Assert.Equal(title, message.Title);
        Assert.Equal(created!.PublishedAt, message.PublishedAt);
    }

    private async Task LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Demo1234!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
