using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.ProductId);
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(150);
        builder.Property(i => i.Quantity).HasPrecision(18, 2);
        builder.Property(i => i.ReturnedQuantity).HasPrecision(18, 2);
        builder.Property(i => i.PerformerUserId);
        builder.Property(i => i.PerformerRole).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(i => i.RowVersion).IsConcurrencyToken();
    }
}
