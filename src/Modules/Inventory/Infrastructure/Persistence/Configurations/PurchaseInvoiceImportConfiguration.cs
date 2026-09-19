using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class PurchaseInvoiceImportConfiguration : IEntityTypeConfiguration<PurchaseInvoiceImport>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceImport> builder)
    {
        builder.ToTable("PurchaseInvoiceImports");
        builder.HasKey(i => i.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", nameof(PurchaseInvoiceImport.AccessKey)).IsUnique();

        builder.Property(i => i.AccessKey).IsRequired().HasMaxLength(44);
        builder.Property(i => i.EmitterLegalName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.EmitterTradeName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.EmitterDocument).IsRequired().HasMaxLength(14);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(20);
        builder.Property(i => i.InvoiceSeries).IsRequired().HasMaxLength(10);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.BlobKey).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.ApIntegrationStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();

        builder.HasMany(i => i.Lines)
            .WithOne()
            .HasForeignKey(l => l.PurchaseInvoiceImportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseInvoiceImportLineConfiguration : IEntityTypeConfiguration<PurchaseInvoiceImportLine>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceImportLine> builder)
    {
        builder.ToTable("PurchaseInvoiceImportLines");
        builder.HasKey(l => l.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(l => l.SupplierProductCode).IsRequired().HasMaxLength(60);
        builder.Property(l => l.Barcode).HasMaxLength(50);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(500);
        builder.Property(l => l.Ncm).IsRequired().HasMaxLength(8);
        builder.Property(l => l.UnitOfMeasure).IsRequired().HasMaxLength(20);
        builder.Property(l => l.Quantity).HasPrecision(18, 4);
        builder.Property(l => l.UnitCost).HasPrecision(18, 4);
        builder.Property(l => l.LineTotal).HasPrecision(18, 2);
        builder.Property(l => l.LotNumber).HasMaxLength(50);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
    }
}

public class SupplierProductMappingConfiguration : IEntityTypeConfiguration<SupplierProductMapping>
{
    public void Configure(EntityTypeBuilder<SupplierProductMapping> builder)
    {
        builder.ToTable("SupplierProductMappings");
        builder.HasKey(m => m.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", nameof(SupplierProductMapping.SupplierId), nameof(SupplierProductMapping.SupplierProductCode)).IsUnique();
        builder.Property(m => m.SupplierProductCode).IsRequired().HasMaxLength(60);
        builder.Property(e => e.RowVersion).IsConcurrencyToken();
    }
}
