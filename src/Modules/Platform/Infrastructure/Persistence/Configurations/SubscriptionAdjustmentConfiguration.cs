using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionAdjustmentConfiguration : IEntityTypeConfiguration<SubscriptionAdjustment>
{
    public void Configure(EntityTypeBuilder<SubscriptionAdjustment> builder)
    {
        builder.ToTable("PlatformSubscriptionAdjustments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Amount).HasPrecision(18, 2);
        builder.Property(a => a.Status).HasConversion<int>();
        builder.Property(a => a.RowVersion).IsConcurrencyToken();
        builder.HasIndex(a => a.TenantId);
    }
}
