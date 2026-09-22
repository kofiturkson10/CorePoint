using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Data;

// Exists only so "dotnet ef migrations add --context SqlServerAppDbContext" has a distinct
// context type to tag its migrations with. That keeps the SQL Server migrations (Migrations/SqlServer)
// separate from the SQLite ones (Migrations/), which use different column types and can't be shared.
// The running app always uses the base AppDbContext - see Program.cs.
public class SqlServerAppDbContext : AppDbContext
{
    public SqlServerAppDbContext(DbContextOptions<SqlServerAppDbContext> options) : base(options)
    {
    }
}
