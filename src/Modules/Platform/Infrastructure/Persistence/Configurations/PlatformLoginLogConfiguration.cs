using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for platform login logs.</summary>
internal sealed class PlatformLoginLogConfiguration : IEntityTypeConfiguration<PlatformLoginLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlatformLoginLog> builder)
    {
        builder.ToTable("PlatformLoginLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.ClientIp).HasMaxLength(64).IsRequired();
        builder.Property(e => e.UserAgent).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Country).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Region).HasMaxLength(128).IsRequired();
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => e.TenantId);
    }
}
