using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
{
    public void Configure(EntityTypeBuilder<CommissionRule> builder)
    {
        builder.ToTable("CommissionRules");
        builder.HasKey(r => r.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(r => r.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.AppliesTo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.RatePercent).HasPrecision(5, 2);
        builder.HasIndex("TenantId", nameof(CommissionRule.Role), nameof(CommissionRule.AppliesTo)).IsUnique();
        builder.Property(r => r.RowVersion).IsConcurrencyToken();
    }
}
