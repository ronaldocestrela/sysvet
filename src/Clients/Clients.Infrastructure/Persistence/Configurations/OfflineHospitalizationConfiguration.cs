using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineWardUnitConfiguration : IEntityTypeConfiguration<WardUnit>
{
    public void Configure(EntityTypeBuilder<WardUnit> builder)
    {
        builder.ToTable("WardUnits");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(200);
        builder.HasMany(u => u.Beds).WithOne().HasForeignKey(b => b.WardUnitId);
        builder.Metadata.FindNavigation(nameof(WardUnit.Beds))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class OfflineBedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.ToTable("Beds");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Code).HasMaxLength(20);
    }
}

internal sealed class OfflineHospitalizationConfiguration : IEntityTypeConfiguration<Hospitalization>
{
    public void Configure(EntityTypeBuilder<Hospitalization> builder)
    {
        builder.ToTable("Hospitalizations");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Reason).HasMaxLength(1000);
        builder.HasMany(h => h.MedicationOrders).WithOne().HasForeignKey(o => o.HospitalizationId);
        builder.HasMany(h => h.Administrations).WithOne().HasForeignKey(a => a.HospitalizationId);
        builder.HasMany(h => h.ProgressNotes).WithOne().HasForeignKey(n => n.HospitalizationId);
        builder.HasMany(h => h.Procedures).WithOne().HasForeignKey(p => p.HospitalizationId);
        builder.Metadata.FindNavigation(nameof(Hospitalization.MedicationOrders))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Hospitalization.Administrations))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Hospitalization.ProgressNotes))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Hospitalization.Procedures))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class OfflineHospitalMedicationOrderConfiguration : IEntityTypeConfiguration<HospitalMedicationOrder>
{
    public void Configure(EntityTypeBuilder<HospitalMedicationOrder> builder)
    {
        builder.ToTable("HospitalMedicationOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.MedicationName).HasMaxLength(200);
        builder.Property(o => o.Dose).HasMaxLength(100);
        builder.Property(o => o.Route).HasMaxLength(50);
        builder.Property(o => o.DailyTimesCsv).HasMaxLength(200);
    }
}

internal sealed class OfflineMedicationAdministrationConfiguration : IEntityTypeConfiguration<MedicationAdministration>
{
    public void Configure(EntityTypeBuilder<MedicationAdministration> builder)
    {
        builder.ToTable("MedicationAdministrations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Notes).HasMaxLength(1000);
    }
}

internal sealed class OfflineHospitalizationProgressNoteConfiguration : IEntityTypeConfiguration<HospitalizationProgressNote>
{
    public void Configure(EntityTypeBuilder<HospitalizationProgressNote> builder)
    {
        builder.ToTable("HospitalizationProgressNotes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Text).IsRequired().HasMaxLength(4000);
    }
}

internal sealed class OfflineHospitalProcedureConfiguration : IEntityTypeConfiguration<HospitalProcedure>
{
    public void Configure(EntityTypeBuilder<HospitalProcedure> builder)
    {
        builder.ToTable("HospitalProcedures");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Notes).HasMaxLength(1000);
    }
}
