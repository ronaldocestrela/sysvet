using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class PrescriptionTemplateConfiguration : IEntityTypeConfiguration<PrescriptionTemplate>
{
    public void Configure(EntityTypeBuilder<PrescriptionTemplate> builder)
    {
        builder.ToTable("PrescriptionTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Species).IsRequired().HasMaxLength(100);
        builder.Property(t => t.IsActive).IsRequired();
        builder.HasMany(t => t.Items)
            .WithOne()
            .HasForeignKey(i => i.PrescriptionTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(PrescriptionTemplate.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class PrescriptionTemplateItemConfiguration : IEntityTypeConfiguration<PrescriptionTemplateItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionTemplateItem> builder)
    {
        builder.ToTable("PrescriptionTemplateItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.MedicationName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Concentration).HasMaxLength(200);
        builder.Property(i => i.Dose).HasMaxLength(200);
        builder.Property(i => i.Route).HasMaxLength(100);
        builder.Property(i => i.Frequency).HasMaxLength(200);
        builder.Property(i => i.Duration).HasMaxLength(200);
        builder.Property(i => i.Instructions).HasMaxLength(1000);
        builder.HasIndex(i => i.PrescriptionTemplateId);
    }
}
