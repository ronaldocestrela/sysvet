using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineVaccineProtocolConfiguration : IEntityTypeConfiguration<VaccineProtocol>
{
    public void Configure(EntityTypeBuilder<VaccineProtocol> builder)
    {
        builder.ToTable("VaccineProtocols");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Species).HasConversion<string>().HasMaxLength(30);
        builder.HasMany(p => p.Doses).WithOne().HasForeignKey(d => d.VaccineProtocolId);
        builder.Metadata.FindNavigation(nameof(VaccineProtocol.Doses))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class OfflineVaccineProtocolDoseConfiguration : IEntityTypeConfiguration<VaccineProtocolDose>
{
    public void Configure(EntityTypeBuilder<VaccineProtocolDose> builder)
    {
        builder.ToTable("VaccineProtocolDoses");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Label).HasMaxLength(100);
    }
}

internal sealed class OfflineVaccineDoseConfiguration : IEntityTypeConfiguration<VaccineDose>
{
    public void Configure(EntityTypeBuilder<VaccineDose> builder)
    {
        builder.ToTable("VaccineDoses");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(100);
        builder.Property(d => d.BatchNumber).HasMaxLength(50);
        builder.HasIndex(d => d.PetId);
        builder.HasIndex(d => d.NextDueDate);
    }
}
