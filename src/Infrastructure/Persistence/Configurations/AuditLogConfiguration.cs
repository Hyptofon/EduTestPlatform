using Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        // Details зберігаємо як JSONB, оскільки там може бути різна інфо про зміни
        // Але оскільки це просто рядок в Entity, тут спеціального мапінгу не треба, 
        // або можна налаштувати тип column type явно, якщо PostgreSQL driver не підхопить
        builder.Property(x => x.Details).HasColumnType("jsonb");
    }
}