using Core.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="AuditLog"/> to the Core module schema for append-only audit entries.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(a => a.PayloadSummary)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.HasIndex(a => new { a.TenantId, a.EntityName, a.OccurredAt });
        builder.HasIndex(a => new { a.TenantId, a.EntityId, a.OccurredAt });
    }
}
