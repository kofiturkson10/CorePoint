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

public class EmployeesPagingTests : IClassFixture<TestingWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public EmployeesPagingTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Search_ByName_ReturnsOnlyMatchingEmployee()
    {
        await LoginAsync("admin@company.test");
        var marker = Guid.NewGuid().ToString("N")[..8];
        await CreateEmployeeAsync($"Alice {marker}", $"alice-{marker}@company.test");
        await CreateEmployeeAsync($"Bob {marker}", $"bob-{marker}@company.test");

        var response = await _client.GetAsync($"/api/employees?search={Uri.EscapeDataString($"Alice {marker}")}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Employee>>(JsonOptions);
        var employee = Assert.Single(result!.Items);
        Assert.Equal($"Alice {marker}", employee.Name);
    }

    [Fact]
    public async Task Pagination_ReturnsCorrectPageAndTotals()
    {
        await LoginAsync("admin@company.test");
        // A unique marker in every name/email means "search=marker" only ever matches the five
        // employees created here, regardless of what other tests in this class also created.
        var marker = Guid.NewGuid().ToString("N")[..8];
        for (var i = 1; i <= 5; i++)
        {
            await CreateEmployeeAsync($"{marker} Employee {i:D2}", $"{marker}-{i}@company.test");
        }

        var response = await _client.GetAsync($"/api/employees?search={marker}&page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Employee>>(JsonOptions);
        Assert.Equal(5, result!.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task InvalidPageAndPageSize_FallBackToDefaults()
    {
        await LoginAsync("admin@company.test");

        var response = await _client.GetAsync("/api/employees?page=0&pageSize=-5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Employee>>(JsonOptions);
        Assert.Equal(1, result!.Page);
        Assert.Equal(10, result.PageSize);
    }

    private async Task CreateEmployeeAsync(string name, string email)
    {
        var response = await _client.PostAsJsonAsync("/api/employees", new
        {
            Name = name,
            Department = "IT",
            Role = "Developer",
            Email = email
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Demo1234!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
