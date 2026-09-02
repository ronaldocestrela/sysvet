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
        
        builder.Property(h => h.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(h => h.AdmittedAt).IsRequired();
        builder.Property(h => h.DischargedAt);
        builder.Property(h => h.Status).IsRequired().HasConversion<int>();

        builder.HasMany(h => h.PrescriptionExecutions)
            .WithOne()
            .HasForeignKey(pe => pe.HospitalizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
