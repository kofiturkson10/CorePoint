using CompanyPortal.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CompanyPortal.Api.Tests;

// Byter ut den riktiga SQLite-filen (companyportal.db, med Data:Source i appsettings.json)
// mot en tom databas i minnet, så testerna aldrig läser eller skriver i databasen som
// används när man kör appen lokalt, och alltid startar från ett känt, tomt schema.
public class TestingWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            // Sqlite-anslutningen i minnet försvinner så fort den stängs, så den hålls
            // öppen manuellt för hela testkörningen istället för att låta EF Core sköta den.
            _connection.Open();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
