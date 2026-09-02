using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class ProductBalanceConfiguration : IEntityTypeConfiguration<ProductBalance>
{
    public void Configure(EntityTypeBuilder<ProductBalance> builder)
    {
        builder.ToTable("ProductBalances");
        builder.HasKey(b => b.Id);

        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", "ProductId").IsUnique();

        builder.Property(b => b.TotalQuantity).HasPrecision(18, 4);

        builder.HasOne<Product>()
               .WithOne()
               .HasForeignKey<ProductBalance>(b => b.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.RowVersion).IsConcurrencyToken();
    }
}
