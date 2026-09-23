using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("PlatformFeatureFlags");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Module).HasConversion<int>();
        builder.Property(f => f.State).HasConversion<int>();
        builder.HasIndex(f => new { f.TenantId, f.Module }).IsUnique();
        builder.Property(f => f.RowVersion).IsConcurrencyToken();
    }
}
