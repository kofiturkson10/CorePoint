using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CompanyPortal.Api.Data;

// Lets the "dotnet ef" CLI create a SqlServerAppDbContext without starting the whole app
// (SqlServerAppDbContext isn't registered in Program.cs - only AppDbContext is, at runtime).
//
// For "migrations add", the connection string just needs to be well-formed - EF only uses it to
// generate correct SQL Server syntax, it never has to reach a real database. For "database update"
// it does need to be real and reachable, so it's read from the same environment variable name
// ("ConnectionStrings__SqlServerConnection") that main.bicep sets as an App Service setting - the
// same "__" convention ASP.NET Core uses to map an env var to configuration. Falls back to a local
// placeholder so "migrations add" still works with no setup.
public class SqlServerAppDbContextFactory : IDesignTimeDbContextFactory<SqlServerAppDbContext>
{
    public SqlServerAppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServerConnection")
            ?? "Server=localhost;Database=CompanyPortal;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<SqlServerAppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new SqlServerAppDbContext(optionsBuilder.Options);
    }
}
