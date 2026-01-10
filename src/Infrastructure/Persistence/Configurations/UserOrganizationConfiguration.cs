using Domain.Organizations;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserOrganizationConfiguration : IEntityTypeConfiguration<UserOrganization>
{
    public void Configure(EntityTypeBuilder<UserOrganization> builder)
    {
        builder.ToTable("user_organizations");
        
        builder.HasKey(x => new { x.UserId, x.OrganizationalUnitId });

        // UserId тепер звичайний Guid, конвертація не треба
        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.OrganizationalUnitId)
            .HasConversion(id => id.Value, value => new OrganizationalUnitId(value));

        builder.Property(x => x.Role).HasConversion<string>();
    }
}