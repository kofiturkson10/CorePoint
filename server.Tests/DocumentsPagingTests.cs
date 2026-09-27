using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CompanyPortal.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyPortal.Api.Tests;

public class DocumentsPagingTests : IClassFixture<TestingWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public DocumentsPagingTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Search_ByFileName_ReturnsOnlyMatchingDocument()
    {
        await LoginAsync("admin@company.test");
        var marker = Guid.NewGuid().ToString("N")[..8];
        await UploadDocumentAsync($"{marker}-contract.pdf");
        await UploadDocumentAsync($"{marker}-handbook.docx");

        var response = await _client.GetAsync($"/api/documents?search={marker}-contract");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentInfo>>(JsonOptions);
        var document = Assert.Single(result!.Items);
        Assert.Equal($"{marker}-contract.pdf", document.FileName);
    }

    [Fact]
    public async Task Pagination_ReturnsCorrectPageAndTotals()
    {
        await LoginAsync("admin@company.test");
        // A unique marker in every file name means "search=marker" only ever matches the five
        // documents uploaded here, regardless of what other tests in this class also uploaded.
        var marker = Guid.NewGuid().ToString("N")[..8];
        for (var i = 1; i <= 5; i++)
        {
            await UploadDocumentAsync($"{marker}-file-{i}.txt");
        }

        var response = await _client.GetAsync($"/api/documents?search={marker}&page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentInfo>>(JsonOptions);
        Assert.Equal(5, result!.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task InvalidPageAndPageSize_FallBackToDefaults()
    {
        await LoginAsync("admin@company.test");

        var response = await _client.GetAsync("/api/documents?page=-1&pageSize=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentInfo>>(JsonOptions);
        Assert.Equal(1, result!.Page);
        Assert.Equal(10, result.PageSize);
    }

    private async Task UploadDocumentAsync(string fileName)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", fileName);

        var response = await _client.PostAsync("/api/documents", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Demo1234!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
