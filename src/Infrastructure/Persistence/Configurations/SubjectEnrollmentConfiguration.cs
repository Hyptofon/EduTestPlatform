using Domain.Subjects;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SubjectEnrollmentConfiguration : IEntityTypeConfiguration<SubjectEnrollment>
{
    public void Configure(EntityTypeBuilder<SubjectEnrollment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new SubjectEnrollmentId(x));

        builder.Property(x => x.SubjectId).HasConversion(x => x.Value, x => new SubjectId(x));
        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.EnrolledAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.HasOne(x => x.Subject)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.SubjectId)
            .HasConstraintName("fk_subject_enrollments_subjects_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SubjectId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.SubjectId, x.UserId }).IsUnique();
    }
}