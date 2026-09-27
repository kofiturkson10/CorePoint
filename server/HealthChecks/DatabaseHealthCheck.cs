using CompanyPortal.Api.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CompanyPortal.Api.HealthChecks;

// Works against whichever EF Core provider AppDbContext is configured with (Sqlite locally,
// SqlServer in Azure - see the Database:Provider switch in Program.cs), since it only relies
// on the DbContext being able to reach its database, not on any provider-specific feature.
// That also means it needs no extra NuGet package (e.g. AspNetCore.HealthChecks.SqlServer),
// which would only cover one of the two providers.
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Database connection succeeded.")
            : HealthCheckResult.Unhealthy("Could not connect to the database.");
    }
}
