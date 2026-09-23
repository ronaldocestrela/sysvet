using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class BillingInvoiceConfiguration : IEntityTypeConfiguration<BillingInvoice>
{
    public void Configure(EntityTypeBuilder<BillingInvoice> builder)
    {
        builder.ToTable("PlatformBillingInvoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.Property(i => i.Status).HasConversion<int>();
        builder.Property(i => i.AppliedCouponId);
        builder.HasIndex(i => i.TenantId);
        builder.HasIndex(i => new { i.TenantId, i.Status });
        builder.Property(i => i.RowVersion).IsConcurrencyToken();
        builder.HasMany(i => i.Charges).WithOne(c => c.Invoice!).HasForeignKey(c => c.InvoiceId);
    }
}
