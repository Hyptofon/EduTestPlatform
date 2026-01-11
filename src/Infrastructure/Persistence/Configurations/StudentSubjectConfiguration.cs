using Domain.Enrollments;
using Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StudentSubjectConfiguration : IEntityTypeConfiguration<StudentSubject>
{
    public void Configure(EntityTypeBuilder<StudentSubject> builder)
    {
        builder.ToTable("student_subjects");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new StudentSubjectId(value));

        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.SubjectId)
            .IsRequired()
            .HasConversion(id => id.Value, value => new OrganizationalUnitId(value));

        builder.Property(x => x.EnrolledAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => new { x.StudentId, x.SubjectId })
            .IsUnique();
    }
}