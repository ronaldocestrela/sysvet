using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for partner API keys.</summary>
internal sealed class PartnerApiKeyConfiguration : IEntityTypeConfiguration<PartnerApiKey>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PartnerApiKey> builder)
    {
        builder.ToTable("PlatformPartnerApiKeys");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PartnerName).HasMaxLength(128).IsRequired();
        builder.Property(e => e.KeyPrefix).HasMaxLength(16).IsRequired();
        builder.Property(e => e.SecretHash).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Scope).HasMaxLength(64).IsRequired();
        builder.Property(e => e.CreatedByUserId).HasMaxLength(64).IsRequired();
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
        builder.HasIndex(e => e.SecretHash).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.CreatedAt });
    }
}
