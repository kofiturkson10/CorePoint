using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CompanyPortal.Api.HealthChecks;

// Formats a HealthReport as JSON with a status per check, e.g.:
// { "status": "Healthy", "checks": [ { "name": "database", "status": "Healthy", "duration": "12ms" } ] }
// instead of the default plain-text "Healthy"/"Unhealthy".
public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = $"{entry.Value.Duration.TotalMilliseconds:0}ms"
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
