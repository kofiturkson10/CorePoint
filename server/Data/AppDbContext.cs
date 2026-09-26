using CompanyPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Data;

public class AppDbContext : DbContext
{
    // Non-generic DbContextOptions, not DbContextOptions<AppDbContext>, so that the
    // SqlServerAppDbContext subclass below can pass its own options type through to this constructor.
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    // Each DbSet becomes a table in the database
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<News> News => Set<News>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

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

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.Property(lr => lr.Reason).HasMaxLength(1000);

            // Same SQLite UTC issue as News.PublishedAt above.
            entity.Property(lr => lr.StartDate)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(lr => lr.EndDate)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(lr => lr.CreatedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(lr => lr.ReviewedAt)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            // Looking up "my requests" (EmployeeId) and "pending requests to review" (Status)
            // are the two common queries.
            entity.HasIndex(lr => lr.EmployeeId);
            entity.HasIndex(lr => lr.Status);
        });
    }
}
