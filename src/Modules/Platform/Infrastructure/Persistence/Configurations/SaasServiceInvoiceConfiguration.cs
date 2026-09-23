using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for SaaS NFS-e rows.</summary>
internal sealed class SaasServiceInvoiceConfiguration : IEntityTypeConfiguration<SaasServiceInvoice>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SaasServiceInvoice> builder)
    {
        builder.ToTable("PlatformSaasServiceInvoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.Property(i => i.RecipientCnpj).HasMaxLength(14).IsRequired();
        builder.Property(i => i.RecipientLegalName).HasMaxLength(256).IsRequired();
        builder.Property(i => i.NfseNumber).HasMaxLength(32);
        builder.Property(i => i.AccessKey).HasMaxLength(64);
        builder.Property(i => i.XmlBlobKey).HasMaxLength(512);
        builder.Property(i => i.FailureReason).HasMaxLength(1024);
        builder.Property(i => i.RowVersion).IsConcurrencyToken();
        builder.HasIndex(i => i.BillingInvoiceId).IsUnique();
        builder.HasIndex(i => i.TenantId);
    }
}
