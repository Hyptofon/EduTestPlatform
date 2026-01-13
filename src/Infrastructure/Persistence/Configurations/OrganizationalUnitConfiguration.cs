using Domain.Organizations;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrganizationalUnitConfiguration : IEntityTypeConfiguration<OrganizationalUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationalUnit> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new OrganizationalUnitId(x));

        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.ParentId).HasConversion(
            x => x!.Value, 
            x => new OrganizationalUnitId(x))
            .IsRequired(false);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(x => x.Settings)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Units)
            .HasForeignKey(x => x.OrganizationId)
            .HasConstraintName("fk_organizational_units_organizations_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("fk_organizational_units_parent_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.ParentId);
    }
}