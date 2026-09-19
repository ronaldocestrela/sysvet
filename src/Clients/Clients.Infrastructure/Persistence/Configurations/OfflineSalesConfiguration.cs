using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineSalesOrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Status).HasConversion<string>();
        builder.Property(o => o.FinanceIntegrationStatus).HasConversion<string>();
        builder.Ignore(o => o.TotalAmount);
        builder.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Items).HasField("_items");
        builder.HasMany(o => o.Payments).WithOne().HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Payments).HasField("_payments");
    }
}

internal sealed class OfflineSalesOrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Kind).HasConversion<string>();
        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPrice");
            money.Property(m => m.Currency).HasColumnName("Currency");
        });
        builder.Ignore(i => i.TotalPrice);
    }
}

internal sealed class OfflineSalesPaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("SalesPayments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Method).HasConversion<string>();
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount");
            money.Property(m => m.Currency).HasColumnName("Currency");
        });
    }
}

internal sealed class OfflineSalesCashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.ToTable("SalesCashRegisters");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Status).HasConversion<string>();
        builder.OwnsOne(c => c.OpeningBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("OpeningBalance");
            money.Property(m => m.Currency).HasColumnName("OpeningCurrency");
        });
        builder.OwnsOne(c => c.ClosingBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("ClosingBalance");
            money.Property(m => m.Currency).HasColumnName("ClosingCurrency");
        });
    }
}
