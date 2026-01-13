using Domain.Organizations;
using Domain.Users;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserOrganizationConfiguration : IEntityTypeConfiguration<UserOrganization>
{
    public void Configure(EntityTypeBuilder<UserOrganization> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new UserOrganizationId(x));

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));

        builder.Property(x => x.Role)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_user_organizations_users_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .HasConstraintName("fk_user_organizations_organizations_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => new { x.UserId, x.OrganizationId }).IsUnique();
    }
}