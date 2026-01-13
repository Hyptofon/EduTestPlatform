using Domain.Organizations;
using Domain.Subjects;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new SubjectId(x));

        builder.Property(x => x.OrganizationalUnitId).HasConversion(
            x => x.Value, 
            x => new OrganizationalUnitId(x));

        builder.Property(x => x.CreatedByUserId).IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(x => x.AccessType)
            .HasConversion<string>()
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.AccessKey)
            .HasColumnType("varchar(100)")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.HasOne(x => x.OrganizationalUnit)
            .WithMany()
            .HasForeignKey(x => x.OrganizationalUnitId)
            .HasConstraintName("fk_subjects_organizational_units_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrganizationalUnitId);
        builder.HasIndex(x => x.CreatedByUserId);
    }
}