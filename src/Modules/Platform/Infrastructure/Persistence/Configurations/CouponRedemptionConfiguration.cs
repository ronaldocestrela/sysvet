using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
{
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
        builder.ToTable("PlatformCouponRedemptions");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.TenantId, r.CouponId }).IsUnique();
        builder.HasOne(r => r.Coupon).WithMany().HasForeignKey(r => r.CouponId);
        builder.Property(n => n.RowVersion).IsConcurrencyToken();
    }
}
