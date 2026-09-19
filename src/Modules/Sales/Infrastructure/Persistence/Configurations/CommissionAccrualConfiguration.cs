using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public class CommissionAccrualConfiguration : IEntityTypeConfiguration<CommissionAccrual>
{
    public void Configure(EntityTypeBuilder<CommissionAccrual> builder)
    {
        builder.ToTable("CommissionAccruals");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.RatePercent).HasPrecision(5, 2);
        builder.OwnsOne(c => c.BaseAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("BaseAmount").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("BaseCurrency").HasMaxLength(3);
        });
        builder.OwnsOne(c => c.CommissionAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("CommissionAmount").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("CommissionCurrency").HasMaxLength(3);
        });
    }
}
