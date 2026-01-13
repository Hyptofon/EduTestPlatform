using Domain.Invites;
using Domain.Organizations;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InviteCodeConfiguration : IEntityTypeConfiguration<InviteCode>
{
    public void Configure(EntityTypeBuilder<InviteCode> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new InviteCodeId(x));

        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.OrganizationalUnitId).HasConversion(
            x => x!.Value, 
            x => new OrganizationalUnitId(x))
            .IsRequired(false);

        builder.Property(x => x.Code)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.AssignedRole)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.MaxUses)
            .IsRequired(false);

        builder.Property(x => x.CurrentUses)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .HasConstraintName("fk_invite_codes_organizations_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.OrganizationalUnit)
            .WithMany()
            .HasForeignKey(x => x.OrganizationalUnitId)
            .HasConstraintName("fk_invite_codes_organizational_units_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.IsActive);
    }
}