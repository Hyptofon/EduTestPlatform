using System.Data;
using Domain.Audit;
using Domain.Enrollments;
using Domain.Media;
using Domain.Organizations;
using Domain.Tests;
using Domain.TestSessions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<OrganizationalUnit> OrganizationalUnits { get; }
    DbSet<Test> Tests { get; }
    DbSet<TestSession> TestSessions { get; }
    DbSet<MediaFile> MediaFiles { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<InviteCode> InviteCodes { get; }
    DbSet<StudentSubject> StudentSubjects { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}