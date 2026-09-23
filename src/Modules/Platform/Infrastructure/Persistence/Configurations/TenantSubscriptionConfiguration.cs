using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("PlatformTenantSubscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).HasConversion<int>();
        builder.Property(s => s.TrialEndAction).HasConversion<int>();
        builder.Property(s => s.CreditBalance).HasPrecision(18, 2);
        builder.HasIndex(s => s.TenantId).IsUnique();
        builder.Property(s => s.RowVersion).IsConcurrencyToken();
        builder.HasOne(s => s.Plan).WithMany().HasForeignKey(s => s.PlanId);
        builder.HasMany(s => s.AddOns).WithOne(a => a.Subscription!).HasForeignKey(a => a.SubscriptionId);
    }
}
