using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("PlatformCoupons");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(64);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.DiscountType).HasConversion<int>();
        builder.Property(c => c.Value).HasPrecision(18, 2);
        builder.Property(n => n.RowVersion).IsConcurrencyToken();
    }
}
