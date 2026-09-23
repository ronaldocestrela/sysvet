using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for platform change audit entries.</summary>
internal sealed class PlatformChangeAuditEntryConfiguration : IEntityTypeConfiguration<PlatformChangeAuditEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlatformChangeAuditEntry> builder)
    {
        builder.ToTable("PlatformChangeAuditEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ActorUserId).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(64).IsRequired();
        builder.Property(e => e.PayloadSummary).HasMaxLength(2048).IsRequired();
        builder.Property(e => e.ClientIp).HasMaxLength(64).IsRequired();
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => e.TenantId);
    }
}
