using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for impersonation audit entries (append-only).</summary>
internal sealed class ImpersonationAuditEntryConfiguration : IEntityTypeConfiguration<ImpersonationAuditEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ImpersonationAuditEntry> builder)
    {
        builder.ToTable("PlatformImpersonationAuditEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ActorUserId).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(32).IsRequired();
        builder.Property(e => e.ClientIp).HasMaxLength(64).IsRequired();
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
        builder.HasIndex(e => e.SessionId);
        builder.HasIndex(e => e.OccurredAt);
    }
}
