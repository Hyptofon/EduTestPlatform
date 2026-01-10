using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Identity автоматично налаштовує Id, тому conversion тут НЕ потрібен.
        
        builder.HasMany(x => x.Organizations)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    }
}