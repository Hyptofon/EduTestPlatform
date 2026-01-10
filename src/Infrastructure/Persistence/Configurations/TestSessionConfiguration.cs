using Domain.TestSessions;
using Domain.Tests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TestSessionConfiguration : IEntityTypeConfiguration<TestSession>
{
    public void Configure(EntityTypeBuilder<TestSession> builder)
    {
        builder.ToTable("test_sessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(id => id.Value, value => new TestSessionId(value));
        builder.Property(x => x.TestId).HasConversion(id => id.Value, value => new TestId(value));
        builder.Property(x => x.StudentId).HasConversion(id => id.Value, value => new UserId(value));

        // JSONB для відповідей та логів порушень
        builder.OwnsMany(x => x.Answers, a => { a.ToJson(); });
        builder.OwnsMany(x => x.Violations, v => { v.ToJson(); });
    }
}