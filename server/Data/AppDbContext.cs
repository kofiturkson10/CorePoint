using CompanyPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Each DbSet becomes a table in the database
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<News> News => Set<News>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<News>(entity =>
        {
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Content).IsRequired().HasMaxLength(10000);
            entity.Property(n => n.Author).IsRequired().HasMaxLength(100);

            // SQLite has no real date type and gives DateTime back with Kind = Unspecified,
            // which would be sent to the client without a "Z". Mark it as UTC when reading.
            entity.Property(n => n.PublishedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // The list is sorted newest first
            entity.HasIndex(n => n.PublishedAt);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);

            // Two employees can't share an email address
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
