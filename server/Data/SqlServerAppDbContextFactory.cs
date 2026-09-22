using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CompanyPortal.Api.Data;

// Lets the "dotnet ef" CLI create a SqlServerAppDbContext without starting the whole app
// (SqlServerAppDbContext isn't registered in Program.cs - only AppDbContext is, at runtime).
// The connection string below only needs to be well-formed; EF uses it to generate correct
// SQL Server syntax for migrations, it does not need to point at a real, reachable database.
public class SqlServerAppDbContextFactory : IDesignTimeDbContextFactory<SqlServerAppDbContext>
{
    public SqlServerAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerAppDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=CompanyPortal;Trusted_Connection=True;TrustServerCertificate=True");
        return new SqlServerAppDbContext(optionsBuilder.Options);
    }
}
