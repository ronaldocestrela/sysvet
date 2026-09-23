using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for daily tenant request counters.</summary>
internal sealed class TenantRequestDailyConfiguration : IEntityTypeConfiguration<TenantRequestDaily>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantRequestDaily> builder)
    {
        builder.ToTable("PlatformTenantRequestDailies");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
        builder.HasIndex(e => new { e.TenantId, e.DateUtc }).IsUnique();
    }
}
