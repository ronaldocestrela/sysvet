using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class BillingWebhookReceiptConfiguration : IEntityTypeConfiguration<BillingWebhookReceipt>
{
    public void Configure(EntityTypeBuilder<BillingWebhookReceipt> builder)
    {
        builder.ToTable("PlatformBillingWebhookReceipts");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.IdempotencyKey).IsUnique();
        builder.Property(r => r.IdempotencyKey).HasMaxLength(128);
        builder.Property(r => r.RowVersion).IsConcurrencyToken();
    }
}
