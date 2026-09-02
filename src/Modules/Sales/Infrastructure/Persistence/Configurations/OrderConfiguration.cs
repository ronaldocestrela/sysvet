using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;
using System;

namespace Sales.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        // Shadow property for TenantId mapping
        builder.Property<Guid>("TenantId").IsRequired();
        
        builder.Property(o => o.Status).HasMaxLength(20).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.PaidAt);

        // Ignore computed property
        builder.Ignore(o => o.TotalAmount);

        // Navigation
        builder.HasMany(o => o.Items)
               .WithOne()
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.RowVersion).IsConcurrencyToken();
    }
}
