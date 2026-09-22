using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CompanyPortal.Api.Data;

// Lets "dotnet ef" commands build an AppDbContext without starting the whole app.
//
// This is also required to make "--context" work correctly: when a project has exactly one
// IDesignTimeDbContextFactory, "dotnet ef" uses it for every command and silently ignores
// "--context" - which, before this file existed, meant every "dotnet ef ... --context AppDbContext"
// command was actually run against SqlServerAppDbContextFactory instead. With one factory per
// context, "--context" disambiguates between them correctly.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=companyportal.db");
        return new AppDbContext(optionsBuilder.Options);
    }
}
