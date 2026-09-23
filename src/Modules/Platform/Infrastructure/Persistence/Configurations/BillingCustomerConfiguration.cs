using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class BillingCustomerConfiguration : IEntityTypeConfiguration<BillingCustomer>
{
    public void Configure(EntityTypeBuilder<BillingCustomer> builder)
    {
        builder.ToTable("PlatformBillingCustomers");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.TenantId).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(320);
        builder.Property(c => c.CpfCnpj).HasMaxLength(14);
        builder.Property(c => c.GatewayCustomerId).HasMaxLength(64);
        builder.Property(c => c.RowVersion).IsConcurrencyToken();
    }
}
