using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiscal.Infrastructure.Persistence.Configurations;

internal sealed class FiscalDocumentConfiguration : IEntityTypeConfiguration<FiscalDocument>
{
    public void Configure(EntityTypeBuilder<FiscalDocument> builder)
    {
        builder.ToTable("FiscalDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.AccessKey).HasMaxLength(60);
        builder.Property(x => x.Protocol).HasMaxLength(60);
        builder.Property(x => x.RejectionReason).HasMaxLength(2000);
        builder.Property(x => x.XmlBlobKey).HasMaxLength(500);
        builder.Property(x => x.DanfeBlobKey).HasMaxLength(500);
        builder.Property(x => x.RecipientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RecipientCpf).HasMaxLength(11);
        builder.Property(x => x.RecipientUf).HasMaxLength(2);
        builder.Property(x => x.NfseNumber).HasMaxLength(50);
        builder.Ignore(x => x.TotalAmount);

        builder.HasIndex(x => new { x.SourceOrderId, x.DocumentType, x.Status });

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.FiscalDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Items).HasField("_items");

        builder.HasMany(x => x.Corrections)
            .WithOne()
            .HasForeignKey(c => c.FiscalDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Corrections).HasField("_corrections");
    }
}
