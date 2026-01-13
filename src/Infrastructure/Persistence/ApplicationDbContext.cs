using System.Data;
using System.Reflection;
using Application.Common.Interfaces;
using Domain.Invites;
using Domain.Organizations;
using Domain.Subjects;
using Domain.Tests;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<Organization> Organizations { get; init; }
    public DbSet<OrganizationalUnit> OrganizationalUnits { get; init; }
    public DbSet<InviteCode> InviteCodes { get; init; }
    public DbSet<UserOrganization> UserOrganizations { get; init; }
    
    public DbSet<Subject> Subjects { get; init; }
    public DbSet<SubjectEnrollment> SubjectEnrollments { get; init; }
    
    public DbSet<Test> Tests { get; init; }
    public DbSet<TestSession> TestSessions { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Міняємо назви таблиць для Identity
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
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}