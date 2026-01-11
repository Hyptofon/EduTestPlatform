using Domain.Tests;
using Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.ToTable("tests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(id => id.Value, value => new TestId(value));

        builder.Property(x => x.AuthorId).IsRequired(); // ← Guid, не UserId
        builder.Property(x => x.SubjectId).HasConversion(id => id.Value, value => new OrganizationalUnitId(value));

        builder.OwnsOne(x => x.Content, content =>
        {
            content.ToJson();
            content.OwnsMany(c => c.Sections, section =>
            {
                section.OwnsMany(s => s.Questions, question =>
                {
                    question.OwnsMany(q => q.Options);
                });
            });
        });

        builder.OwnsOne(x => x.Settings);
    }
}