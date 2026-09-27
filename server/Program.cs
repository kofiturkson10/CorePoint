using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using CompanyPortal.Api.Data;
using CompanyPortal.Api.HealthChecks;
using CompanyPortal.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Reads APPLICATIONINSIGHTS_CONNECTION_STRING from configuration/environment automatically.
// Empty locally (see appsettings.Development.json), set via App Service app settings in Azure.
builder.Services.AddApplicationInsightsTelemetry();

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

// Scoped, like AppDbContext itself (AddDbContext registers it as Scoped by default).
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection(BlobStorageOptions.SectionName));

// DefaultAzureCredential = managed identity when running in Azure, and your own login
// (az login / Visual Studio) when running locally. No keys or connection strings in the code.
// The factories run the first time a client is needed, so the rest of the API works even if
// storage isn't configured yet.
builder.Services.AddSingleton(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.ServiceUri))
    {
        throw new InvalidOperationException("BlobStorage:ServiceUri is not configured.");
    }

    return new BlobServiceClient(new Uri(options.ServiceUri), new DefaultAzureCredential());
});
builder.Services.AddSingleton(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
    var serviceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
    return serviceClient.GetBlobContainerClient(options.ContainerName);
});
builder.Services.AddSingleton<IDocumentService, BlobDocumentService>();

builder.Services.Configure<QueueStorageOptions>(builder.Configuration.GetSection(QueueStorageOptions.SectionName));

// Unlike BlobServiceClient above, this is allowed to be null: Documents can't function at all
// without Blob Storage, but a news item can still be created just fine without a working
// notifications queue - see QueueNewsNotificationService, which treats a null QueueClient as
// "notifications are disabled" instead of crashing. Empty locally (see
// appsettings.Development.json) so local development never needs a queue connection.
// Registered via the non-generic Type overload (AddSingleton<QueueClient?> isn't allowed -
// AddSingleton<TService>'s "class" constraint rejects a nullable-annotated type argument).
builder.Services.AddSingleton(typeof(QueueClient), serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<QueueStorageOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.ServiceUri))
    {
        return null!;
    }

    var queueServiceClient = new QueueServiceClient(new Uri(options.ServiceUri), new DefaultAzureCredential());
    return queueServiceClient.GetQueueClient(options.QueueName);
});
builder.Services.AddSingleton<INewsNotificationQueue, QueueNewsNotificationService>();

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

// Only a database check for now: BlobContainerClient is built lazily by a factory that throws
// if BlobStorage:ServiceUri isn't configured (true for the test environment, and for local dev
// unless Azure Storage is set up), and even when it is, a health check would need a real Azure
// call via DefaultAzureCredential - not something the test environment or offline local dev can
// answer reliably. Skipped rather than making this endpoint flaky.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["database"]);

var app = builder.Build();


app.MapOpenApi(); // the OpenAPI spec as JSON: /openapi/v1.json
app.MapScalarApiReference(); // the visual test UI, reads the spec above: /scalar

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// No [Authorize] here (MapHealthChecks endpoints aren't covered by UseAuthorization unless
// asked to be), so Azure App Service can reach it anonymously, same as before.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }