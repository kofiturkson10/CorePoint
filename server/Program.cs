using Azure.Identity;
using Azure.Storage.Blobs;
using CompanyPortal.Api.Data;
using CompanyPortal.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

// "Database:Provider" picks the EF Core provider: "Sqlite" (default, for local development)
// or "SqlServer" (for Azure SQL Database). Same AppDbContext and model either way.
var databaseProvider = builder.Configuration.GetValue("Database:Provider", "Sqlite");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

builder.Services.AddSingleton<IUserService, InMemoryUserService>();

builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection(BlobStorageOptions.SectionName));

// DefaultAzureCredential = managed identity when running in Azure, and your own login
// (az login / Visual Studio) when running locally. No keys or connection strings in the code.
// The factory runs the first time the client is needed, so the rest of the API works even if storage isn't configured yet.
builder.Services.AddSingleton(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.ServiceUri))
    {
        throw new InvalidOperationException("BlobStorage:ServiceUri is not configured.");
    }

    var containerUri = new Uri($"{options.ServiceUri.TrimEnd('/')}/{options.ContainerName}");
    return new BlobContainerClient(containerUri, new DefaultAzureCredential());
});
builder.Services.AddSingleton<IDocumentService, BlobDocumentService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CompanyPortal.Auth";
        options.Cookie.HttpOnly = true; // JavaScript can't read the cookie (protects against XSS theft)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // only sent over HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // This is an API: answer 401/403 instead of redirecting to a login page.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // the OpenAPI spec as JSON: /openapi/v1.json
    app.MapScalarApiReference(); // the visual test UI, reads the spec above: /scalar
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
