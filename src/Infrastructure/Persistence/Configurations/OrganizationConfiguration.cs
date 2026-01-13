using Domain.Organizations;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new OrganizationId(x));

        builder.Property(x => x.Name)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(x => x.LogoUrl)
            .HasColumnType("varchar(500)")
            .IsRequired(false);

        builder.Property(x => x.HeroImageUrl)
            .HasColumnType("varchar(500)")
            .IsRequired(false);

        builder.Property(x => x.WelcomeText)
            .HasColumnType("varchar(1000)")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}