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
        builder.HasIndex("TenantId", "Sku").IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(40);
        builder.Property(p => p.Barcode).IsRequired().HasMaxLength(50);
        builder.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(20);
        builder.Property(p => p.ReorderLevel).HasPrecision(18, 2);
        builder.Property(p => p.TargetStock).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Ncm).IsRequired().HasMaxLength(8);
        builder.Property(p => p.Cest).HasMaxLength(7);
        builder.Property(p => p.AverageCost).HasPrecision(18, 4);
        builder.Property(p => p.UnitsPerPackage).HasPrecision(18, 4).HasDefaultValue(1m);

        builder.Property(e => e.RowVersion).IsConcurrencyToken();
    }
}
