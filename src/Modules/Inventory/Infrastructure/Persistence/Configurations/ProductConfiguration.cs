using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        // Shadow property for TenantId mapping
        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", "Barcode").IsUnique();
        
        // Multi-tenant Query Filter is handled in DbContext

        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Barcode).IsRequired().HasMaxLength(50);
        builder.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(20);
        builder.Property(p => p.ReorderLevel).HasPrecision(18, 2);

        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
