using Domain.Subjects;
using Domain.Tests;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new TestId(x));

        builder.Property(x => x.SubjectId).HasConversion(x => x.Value, x => new SubjectId(x));
        builder.Property(x => x.CreatedByUserId).IsRequired();

        builder.Property(x => x.Title)
            .HasColumnType("varchar(500)")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("text")
            .IsRequired(false);

        builder.OwnsOne(x => x.Settings, settings =>
        {
            settings.Property(s => s.TimeLimitMinutes).IsRequired(false);
            settings.Property(s => s.StartDate)
                .HasConversion(new DateTimeUtcConverter())
                .IsRequired(false);
            settings.Property(s => s.EndDate)
                .HasConversion(new DateTimeUtcConverter())
                .IsRequired(false);
            settings.Property(s => s.BankModeQuestionCount).IsRequired(false);
            settings.Property(s => s.ShuffleAnswers).IsRequired();
            settings.Property(s => s.ResultDisplayPolicy)
                .HasConversion<string>()
                .IsRequired();
            settings.Property(s => s.MaxAttempts).IsRequired();
            settings.Property(s => s.IsPublic).IsRequired();
        });

        builder.Property(x => x.ContentJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .HasConstraintName("fk_tests_subjects_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SubjectId);
        builder.HasIndex(x => x.CreatedByUserId);
    }
}