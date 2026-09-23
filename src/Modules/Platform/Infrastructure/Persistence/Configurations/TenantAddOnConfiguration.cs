using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class TenantAddOnConfiguration : IEntityTypeConfiguration<TenantAddOn>
{
    public void Configure(EntityTypeBuilder<TenantAddOn> builder)
    {
        builder.ToTable("PlatformTenantAddOns");
        builder.HasKey(a => new { a.SubscriptionId, a.AddOnId });
        builder.HasOne(a => a.AddOn).WithMany().HasForeignKey(a => a.AddOnId);
    }
}
