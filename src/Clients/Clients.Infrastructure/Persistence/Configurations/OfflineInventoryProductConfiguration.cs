using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineInventoryProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Category).HasConversion<string>();
    }
}

internal sealed class OfflineProductLotConfiguration : IEntityTypeConfiguration<ProductLot>
{
    public void Configure(EntityTypeBuilder<ProductLot> builder)
    {
        builder.ToTable("ProductLots");
        builder.HasKey(l => l.Id);
    }
}

internal sealed class OfflineProductBalanceConfiguration : IEntityTypeConfiguration<ProductBalance>
{
    public void Configure(EntityTypeBuilder<ProductBalance> builder)
    {
        builder.ToTable("ProductBalances");
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => b.ProductId).IsUnique();
    }
}

internal sealed class OfflineSupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);
    }
}
