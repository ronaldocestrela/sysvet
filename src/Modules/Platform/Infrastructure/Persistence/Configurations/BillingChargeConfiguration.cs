using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class BillingChargeConfiguration : IEntityTypeConfiguration<BillingCharge>
{
    public void Configure(EntityTypeBuilder<BillingCharge> builder)
    {
        builder.ToTable("PlatformBillingCharges");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.GatewayPaymentId).HasMaxLength(64);
        builder.Property(c => c.PixCopyPaste).HasMaxLength(2048);
        builder.Property(c => c.BoletoIdentificationField).HasMaxLength(256);
        builder.Property(c => c.RowVersion).IsConcurrencyToken();
    }
}
