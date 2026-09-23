using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class BillingPaymentMethodConfiguration : IEntityTypeConfiguration<BillingPaymentMethod>
{
    public void Configure(EntityTypeBuilder<BillingPaymentMethod> builder)
    {
        builder.ToTable("PlatformBillingPaymentMethods");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.TenantId).IsUnique();
        builder.Property(m => m.Kind).HasConversion<int>();
        builder.Property(m => m.CreditCardToken).HasMaxLength(128);
        builder.Property(m => m.RowVersion).IsConcurrencyToken();
    }
}
