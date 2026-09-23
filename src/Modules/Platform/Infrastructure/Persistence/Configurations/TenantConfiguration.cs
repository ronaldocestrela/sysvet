using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for global tenant registry.</summary>
internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("PlatformTenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Slug).HasMaxLength(63).IsRequired();
        builder.Property(t => t.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(t => t.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(t => t.Slug);
        builder.Property(t => t.SchemaName).HasMaxLength(128).IsRequired();
        builder.Property(t => t.RowVersion).IsConcurrencyToken();
        builder.Property(t => t.CancelledAt);
    }
}
