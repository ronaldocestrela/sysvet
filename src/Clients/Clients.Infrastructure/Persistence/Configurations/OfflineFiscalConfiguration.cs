using Clients.Infrastructure.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineFiscalIssuerCacheConfiguration : IEntityTypeConfiguration<OfflineFiscalIssuerCache>
{
    public void Configure(EntityTypeBuilder<OfflineFiscalIssuerCache> builder)
    {
        builder.ToTable("FiscalIssuerCache");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LegalName).HasMaxLength(200);
        builder.Property(x => x.Cnpj).HasMaxLength(14);
        builder.Property(x => x.State).HasMaxLength(2);
    }
}

internal sealed class OfflineFiscalSequenceConfiguration : IEntityTypeConfiguration<OfflineFiscalSequence>
{
    public void Configure(EntityTypeBuilder<OfflineFiscalSequence> builder)
    {
        builder.ToTable("FiscalSequences");
        builder.HasKey(x => x.Id);
    }
}

internal sealed class OfflineFiscalDocumentConfiguration : IEntityTypeConfiguration<OfflineFiscalDocument>
{
    public void Configure(EntityTypeBuilder<OfflineFiscalDocument> builder)
    {
        builder.ToTable("FiscalDocuments");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OrderId);
        builder.Property(x => x.AccessKey).HasMaxLength(60);
        builder.Property(x => x.QrCodeUrl).HasMaxLength(2000);
    }
}
