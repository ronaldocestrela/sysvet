using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;
using Fiscal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiscal.Infrastructure.Persistence.Configurations;

internal sealed class IssuerProfileConfiguration : IEntityTypeConfiguration<IssuerProfile>
{
    public void Configure(EntityTypeBuilder<IssuerProfile> builder)
    {
        builder.ToTable("IssuerProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TradeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Cnpj)
            .HasConversion(v => v.Value, v => FiscalCnpj.Create(v).Value)
            .HasColumnName("Cnpj")
            .HasMaxLength(14)
            .IsRequired();
        builder.Property(x => x.StateRegistration).HasMaxLength(20);
        builder.Property(x => x.MunicipalRegistration).HasMaxLength(20);
        builder.Property(x => x.Cnae).HasMaxLength(10);
        builder.Property(x => x.Street).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Number).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Complement).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.State).HasMaxLength(2).IsRequired();
        builder.Property(x => x.PostalCode).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.NationalServiceTaxCode).HasMaxLength(20);
        builder.Property(x => x.DefaultIssRate).HasPrecision(5, 2);
        builder.Property(x => x.Environment).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CertificateBlobKey).HasMaxLength(500);
        builder.Property(x => x.EncryptedCertificatePassword).HasMaxLength(500);
        builder.Property(x => x.NextNfeNumber).IsConcurrencyToken();
        builder.Property(x => x.NextNfceNumber).IsConcurrencyToken();
    }
}
