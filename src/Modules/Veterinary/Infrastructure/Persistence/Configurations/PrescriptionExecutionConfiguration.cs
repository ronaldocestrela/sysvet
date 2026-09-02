using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class PrescriptionExecutionConfiguration : IEntityTypeConfiguration<PrescriptionExecution>
{
    public void Configure(EntityTypeBuilder<PrescriptionExecution> builder)
    {
        builder.ToTable("PrescriptionExecutions");

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.HospitalizationId).IsRequired();
        
        builder.Property(p => p.MedicationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Dose)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.Property(p => p.ExecutedAt).IsRequired();
        builder.Property(p => p.ExecutedBy).IsRequired();
    }
}
