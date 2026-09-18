using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class EvolutionNoteConfiguration : IEntityTypeConfiguration<EvolutionNote>
{
    public void Configure(EntityTypeBuilder<EvolutionNote> builder)
    {
        builder.ToTable("EvolutionNotes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.MedicalRecordId).IsRequired();
        builder.Property(n => n.AuthorId).IsRequired();
        builder.Property(n => n.Text).IsRequired().HasMaxLength(4000);
        builder.Property(n => n.RecordedAt).IsRequired();

        builder.HasIndex(n => n.MedicalRecordId);
    }
}
