using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public sealed class ServicePackageConfiguration : IEntityTypeConfiguration<ServicePackage>
{
    public void Configure(EntityTypeBuilder<ServicePackage> builder)
    {
        builder.ToTable("ServicePackages");
        builder.HasKey(p => p.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.ServiceCode).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.RowVersion).IsConcurrencyToken();
    }
}
