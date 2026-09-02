using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;
using System;

namespace Sales.Infrastructure.Persistence.Configurations;

public class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.ToTable("CashRegisters");
        builder.HasKey(c => c.Id);

        // Shadow property for TenantId mapping
        builder.Property<Guid>("TenantId").IsRequired();

        builder.Property(c => c.Status).HasMaxLength(20).IsRequired();

        builder.OwnsOne(c => c.OpeningBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("OpeningBalance").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("OpeningBalanceCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(c => c.ClosingBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("ClosingBalance").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("ClosingBalanceCurrency").HasMaxLength(3);
        });
        
        builder.Property(c => c.RowVersion).IsConcurrencyToken();
    }
}
