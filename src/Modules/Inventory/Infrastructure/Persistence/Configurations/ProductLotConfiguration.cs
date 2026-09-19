using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class ProductLotConfiguration : IEntityTypeConfiguration<ProductLot>
{
    public void Configure(EntityTypeBuilder<ProductLot> builder)
    {
        builder.ToTable("ProductLots");
        builder.HasKey(l => l.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", nameof(ProductLot.ProductId), nameof(ProductLot.LotNumber)).IsUnique();
        builder.Property(l => l.LotNumber).IsRequired().HasMaxLength(50);
        builder.Property(l => l.UnitCost).HasPrecision(18, 4);
        builder.Property(l => l.Quantity).HasPrecision(18, 4);
        builder.Property(l => l.IsFractional).HasDefaultValue(false);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
        builder.HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
