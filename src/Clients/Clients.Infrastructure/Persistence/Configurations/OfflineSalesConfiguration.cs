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
        builder.Property(o => o.SellerUserId).IsRequired();
        builder.Property(o => o.DiscountPercent);
        builder.Ignore(o => o.SubtotalAmount);
        builder.Ignore(o => o.DiscountAmount);
        builder.Ignore(o => o.TotalAmount);
        builder.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Items).HasField("_items");
        builder.HasMany(o => o.Payments).WithOne().HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Payments).HasField("_payments");
        builder.HasMany(o => o.Commissions).WithOne().HasForeignKey(c => c.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Commissions).HasField("_commissions");
        builder.HasMany(o => o.Returns).WithOne().HasForeignKey(r => r.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Returns).HasField("_returns");
    }
}

internal sealed class OfflineSalesOrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Kind).HasConversion<string>();
        builder.Property(i => i.ReturnedQuantity);
        builder.Property(i => i.PerformerUserId);
        builder.Property(i => i.PerformerRole).HasConversion<string>();
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
        builder.Property(p => p.Nsu).HasMaxLength(32);
        builder.Property(p => p.AuthorizationCode).HasMaxLength(32);
        builder.Property(p => p.Provider).HasMaxLength(64);
        builder.Property(p => p.TerminalId).HasMaxLength(64);
        builder.Property(p => p.Brand).HasMaxLength(32);
        builder.Property(p => p.Installments).HasDefaultValue(1);
        builder.HasMany(p => p.Refunds)
            .WithOne()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount");
            money.Property(m => m.Currency).HasColumnName("Currency");
        });
    }
}

internal sealed class OfflineSalesPaymentRefundConfiguration : IEntityTypeConfiguration<PaymentRefund>
{
    public void Configure(EntityTypeBuilder<PaymentRefund> builder)
    {
        builder.ToTable("SalesPaymentRefunds");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RefundNsu).HasMaxLength(32);
        builder.OwnsOne(r => r.Amount, money =>
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

internal sealed class OfflineSalesCommissionAccrualConfiguration : IEntityTypeConfiguration<CommissionAccrual>
{
    public void Configure(EntityTypeBuilder<CommissionAccrual> builder)
    {
        builder.ToTable("SalesCommissionAccruals");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Role).HasConversion<string>();
        builder.Property(c => c.Status).HasConversion<string>();
        builder.OwnsOne(c => c.BaseAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("BaseAmount");
            m.Property(x => x.Currency).HasColumnName("BaseCurrency");
        });
        builder.OwnsOne(c => c.CommissionAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("CommissionAmount");
            m.Property(x => x.Currency).HasColumnName("CommissionCurrency");
        });
    }
}

internal sealed class OfflineSalesSaleReturnConfiguration : IEntityTypeConfiguration<SaleReturn>
{
    public void Configure(EntityTypeBuilder<SaleReturn> builder)
    {
        builder.ToTable("SalesSaleReturns");
        builder.HasKey(r => r.Id);
        builder.OwnsOne(r => r.RefundAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("RefundAmount");
            m.Property(x => x.Currency).HasColumnName("RefundCurrency");
        });
        builder.HasMany(r => r.Lines).WithOne().HasForeignKey(l => l.SaleReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Lines).HasField("_lines");
    }
}

internal sealed class OfflineSalesSaleReturnLineConfiguration : IEntityTypeConfiguration<SaleReturnLine>
{
    public void Configure(EntityTypeBuilder<SaleReturnLine> builder)
    {
        builder.ToTable("SalesSaleReturnLines");
        builder.HasKey(l => l.Id);
    }
}

internal sealed class OfflineSalesCommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
{
    public void Configure(EntityTypeBuilder<CommissionRule> builder)
    {
        builder.ToTable("SalesCommissionRules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Role).HasConversion<string>();
        builder.Property(r => r.AppliesTo).HasConversion<string>();
        builder.HasIndex(nameof(CommissionRule.Role), nameof(CommissionRule.AppliesTo)).IsUnique();
    }
}
