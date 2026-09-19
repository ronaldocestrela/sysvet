using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class InventoryCountConfiguration : IEntityTypeConfiguration<InventoryCount>
{
    public void Configure(EntityTypeBuilder<InventoryCount> builder)
    {
        builder.ToTable("InventoryCounts");
        builder.HasKey(c => c.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", nameof(InventoryCount.Code)).IsUnique();
        builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();

        builder.HasMany(c => c.Lines)
            .WithOne()
            .HasForeignKey(l => l.InventoryCountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InventoryCountLineConfiguration : IEntityTypeConfiguration<InventoryCountLine>
{
    public void Configure(EntityTypeBuilder<InventoryCountLine> builder)
    {
        builder.ToTable("InventoryCountLines");
        builder.HasKey(l => l.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(l => l.CountedQuantity).HasPrecision(18, 4);
        builder.Property(l => l.ExpectedQuantity).HasPrecision(18, 4);
        builder.Property(l => l.Variance).HasPrecision(18, 4);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
        builder.HasIndex(nameof(InventoryCountLine.InventoryCountId), nameof(InventoryCountLine.ProductId), nameof(InventoryCountLine.ProductLotId));
    }
}
