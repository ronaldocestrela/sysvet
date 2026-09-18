using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class ClinicalExamConfiguration : IEntityTypeConfiguration<ClinicalExam>
{
    public void Configure(EntityTypeBuilder<ClinicalExam> builder)
    {
        builder.ToTable("ClinicalExams");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Category).IsRequired().HasConversion<int>();
        builder.Property(e => e.Status).IsRequired().HasConversion<int>();
        builder.Property(e => e.ResultSummary).IsRequired().HasMaxLength(4000);
        builder.HasIndex(e => e.AppointmentId);
        builder.HasIndex(e => e.PetId);
    }
}
