using System.Data;
using System.Reflection;
using Application.Common.Interfaces;
using Infrastructure.Persistence.Converters;
using Domain.Audit;
using Domain.Media;
using Domain.Organizations;
using Domain.Tests;
using Domain.TestSessions;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<OrganizationalUnit> OrganizationalUnits { get; init; }
    public DbSet<Test> Tests { get; init; }
    public DbSet<TestSession> TestSessions { get; init; }
    public DbSet<MediaFile> MediaFiles { get; init; }
    public DbSet<AuditLog> AuditLogs { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }

    // Явна реалізація інтерфейсу IApplicationDbContext для Identity таблиць
    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public new DbSet<ApplicationRole> Roles => Set<ApplicationRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Застосування Converter для всіх DateTime полів (як в репозиторії)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new DateTimeUtcConverter());
                }
            }
        }

        // Identity Tables naming convention
        modelBuilder.Entity<ApplicationUser>().ToTable("users");
        modelBuilder.Entity<ApplicationRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionWrapper(transaction);
    }
}