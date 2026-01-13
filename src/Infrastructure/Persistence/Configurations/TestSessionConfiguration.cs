using Domain.Tests;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TestSessionConfiguration : IEntityTypeConfiguration<TestSession>
{
    public void Configure(EntityTypeBuilder<TestSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new TestSessionId(x));

        builder.Property(x => x.TestId).HasConversion(x => x.Value, x => new TestId(x));
        builder.Property(x => x.StudentId).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.FinishedAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.Answers)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<StudentAnswer>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<StudentAnswer>())
            .IsRequired();

        builder.Property(x => x.Violations)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Violation>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Violation>())
            .IsRequired();

        builder.Property(x => x.TotalScore).IsRequired();
        builder.Property(x => x.MaxScore).IsRequired();

        builder.Property(x => x.TeacherComment)
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasOne(x => x.Test)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.TestId)
            .HasConstraintName("fk_test_sessions_tests_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TestId);
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => new { x.TestId, x.StudentId });
        builder.HasIndex(x => x.Status);
    }
}