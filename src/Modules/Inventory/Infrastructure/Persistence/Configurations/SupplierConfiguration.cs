using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", nameof(Supplier.Document)).IsUnique();
        builder.Property(s => s.LegalName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.TradeName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Document).IsRequired().HasMaxLength(14);
        builder.Property(s => s.ContactEmail).HasMaxLength(200);
        builder.Property(s => s.ContactPhone).HasMaxLength(30);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
    }
}
