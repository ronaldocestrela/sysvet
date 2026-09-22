using Commerce.Domain.Entities;
using Commerce.Domain.Enums;
using Commerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for product offers.</summary>
public sealed class ProductOfferConfiguration : IEntityTypeConfiguration<ProductOffer>
{
    public void Configure(EntityTypeBuilder<ProductOffer> builder)
    {
        builder.ToTable("ProductOffers");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ProductId).IsUnique();
        builder.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SalePrice)
            .HasConversion(m => m.Amount, v => Money.FromPersisted(v))
            .HasPrecision(18, 4);
        builder.Property(x => x.ExternalListingId).HasMaxLength(64);
    }
}

/// <summary>EF mapping for online orders.</summary>
public sealed class OnlineOrderConfiguration : IEntityTypeConfiguration<OnlineOrder>
{
    public void Configure(EntityTypeBuilder<OnlineOrder> builder)
    {
        builder.ToTable("OnlineOrders");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ExternalOrderId);
        builder.Property(x => x.BuyerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BuyerPhone).HasMaxLength(30).IsRequired();
        builder.Property(x => x.BuyerEmail).HasMaxLength(200);
        builder.Property(x => x.ExternalOrderId).HasMaxLength(64);
        builder.Property(x => x.Channel).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Fulfillment).HasConversion<int>();
        builder.Ignore(x => x.Lines);
        builder.HasMany<OnlineOrderLine>("_lines")
            .WithOne()
            .HasForeignKey(x => x.OnlineOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>EF mapping for order lines.</summary>
public sealed class OnlineOrderLineConfiguration : IEntityTypeConfiguration<OnlineOrderLine>
{
    public void Configure(EntityTypeBuilder<OnlineOrderLine> builder)
    {
        builder.ToTable("OnlineOrderLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice)
            .HasConversion(m => m.Amount, v => Money.FromPersisted(v))
            .HasPrecision(18, 4);
    }
}

/// <summary>EF mapping for marketplace sync jobs.</summary>
public sealed class MarketplaceSyncJobConfiguration : IEntityTypeConfiguration<MarketplaceSyncJob>
{
    public void Configure(EntityTypeBuilder<MarketplaceSyncJob> builder)
    {
        builder.ToTable("MarketplaceSyncJobs");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.IdempotencyKey);
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Kind).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.LastError).HasMaxLength(2000);
    }
}

/// <summary>EF mapping for Mercado Livre settings singleton.</summary>
public sealed class MercadoLivreSettingsConfiguration : IEntityTypeConfiguration<MercadoLivreSettings>
{
    public void Configure(EntityTypeBuilder<MercadoLivreSettings> builder)
    {
        builder.ToTable("MercadoLivreSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.Key).HasMaxLength(32).IsRequired();
        builder.Property(x => x.AccessToken).HasMaxLength(500);
        builder.Property(x => x.SiteId).HasMaxLength(8).IsRequired();
    }
}
