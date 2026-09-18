using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class HospitalizationConfiguration : IEntityTypeConfiguration<Hospitalization>
{
    public void Configure(EntityTypeBuilder<Hospitalization> builder)
    {
        builder.ToTable("Hospitalizations");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.PetId).IsRequired();
        builder.Property(h => h.VeterinarianId).IsRequired();
        builder.Property(h => h.BedId).IsRequired();

        builder.Property(h => h.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(h => h.AdmittedAt).IsRequired();
        builder.Property(h => h.DischargedAt);
        builder.Property(h => h.Status).IsRequired().HasConversion<int>();

        builder.HasIndex(h => h.BedId)
            .IsUnique()
            .HasFilter("[Status] = 1");

        builder.HasIndex(h => h.PetId)
            .IsUnique()
            .HasFilter("[Status] = 1");

        ConfigureChildren(builder);
    }

    private static void ConfigureChildren(EntityTypeBuilder<Hospitalization> builder)
    {
        builder.HasMany(h => h.MedicationOrders)
            .WithOne()
            .HasForeignKey(o => o.HospitalizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Hospitalization.MedicationOrders))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(h => h.Administrations)
            .WithOne()
            .HasForeignKey(a => a.HospitalizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Hospitalization.Administrations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(h => h.ProgressNotes)
            .WithOne()
            .HasForeignKey(n => n.HospitalizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Hospitalization.ProgressNotes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(h => h.Procedures)
            .WithOne()
            .HasForeignKey(p => p.HospitalizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Hospitalization.Procedures))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class HospitalMedicationOrderConfiguration : IEntityTypeConfiguration<HospitalMedicationOrder>
{
    public void Configure(EntityTypeBuilder<HospitalMedicationOrder> builder)
    {
        builder.ToTable("HospitalMedicationOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.MedicationName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Dose).HasMaxLength(100);
        builder.Property(o => o.Route).HasMaxLength(50);
        builder.Property(o => o.DailyTimesCsv).IsRequired().HasMaxLength(200);
        builder.Property(o => o.StartsOn)
            .IsRequired()
            .HasConversion(v => v.ToString("yyyy-MM-dd"), v => DateOnly.Parse(v));
        builder.Property(o => o.EndsOn)
            .IsRequired()
            .HasConversion(v => v.ToString("yyyy-MM-dd"), v => DateOnly.Parse(v));
        builder.Property(o => o.Status).IsRequired().HasConversion<int>();
        builder.HasIndex(o => o.HospitalizationId);
    }
}

internal sealed class MedicationAdministrationConfiguration : IEntityTypeConfiguration<MedicationAdministration>
{
    public void Configure(EntityTypeBuilder<MedicationAdministration> builder)
    {
        builder.ToTable("MedicationAdministrations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ScheduledAt).IsRequired();
        builder.Property(a => a.Status).IsRequired().HasConversion<int>();
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.HasIndex(a => a.HospitalizationId);
        builder.HasIndex(a => a.MedicationOrderId);
        builder.HasIndex(a => new { a.HospitalizationId, a.ScheduledAt });
    }
}

internal sealed class HospitalizationProgressNoteConfiguration : IEntityTypeConfiguration<HospitalizationProgressNote>
{
    public void Configure(EntityTypeBuilder<HospitalizationProgressNote> builder)
    {
        builder.ToTable("HospitalizationProgressNotes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Text).IsRequired().HasMaxLength(4000);
        builder.Property(n => n.RecordedAt).IsRequired();
        builder.HasIndex(n => n.HospitalizationId);
    }
}

internal sealed class HospitalProcedureConfiguration : IEntityTypeConfiguration<HospitalProcedure>
{
    public void Configure(EntityTypeBuilder<HospitalProcedure> builder)
    {
        builder.ToTable("HospitalProcedures");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Notes).HasMaxLength(1000);
        builder.Property(p => p.PerformedAt).IsRequired();
        builder.HasIndex(p => p.HospitalizationId);
    }
}
