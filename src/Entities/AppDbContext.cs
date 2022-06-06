using System.Diagnostics;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace datotekica.Entities;

public partial class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    // public DbSet<User> Users { get; set; } = null!;
    // public DbSet<UserRole> UserRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // builder.Entity<User>(e =>
        // {
        //     e.HasKey(p => p.UserId);
        //     e.HasMany(p => p.UserRoles).WithOne(p => p.User!).OnDelete(DeleteBehavior.Cascade);
        // });

        // builder.Entity<UserRole>(e =>
        // {
        //     e.HasKey(p => new { p.UserId, p.RoleId });
        // });

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var dtProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in dtProperties)
                builder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(new DateTimeToBinaryConverter());

            var decProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

            foreach (var property in decProperties)
                builder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion<double>();

            var spanProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(TimeSpan) || p.PropertyType == typeof(TimeSpan?));

            foreach (var property in spanProperties)
                builder.Entity(entityType.Name).Property(property.Name).HasConversion<long>();
        }
    }
    public async ValueTask InitializeDefaults()
    {
        // Initial data

        if (!Debugger.IsAttached)
        {
            await SaveChangesAsync();
            return;
        }

        // Additional dev data
    }
}