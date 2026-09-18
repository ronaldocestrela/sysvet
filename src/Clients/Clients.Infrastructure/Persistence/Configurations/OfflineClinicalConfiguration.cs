using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineClinicalExamConfiguration : IEntityTypeConfiguration<ClinicalExam>
{
    public void Configure(EntityTypeBuilder<ClinicalExam> builder)
    {
        builder.ToTable("ClinicalExams");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200);
        builder.Property(e => e.ResultSummary).HasMaxLength(4000);
        builder.HasIndex(e => e.AppointmentId);
    }
}

internal sealed class OfflineIssuedPrescriptionConfiguration : IEntityTypeConfiguration<IssuedPrescription>
{
    public void Configure(EntityTypeBuilder<IssuedPrescription> builder)
    {
        builder.ToTable("IssuedPrescriptions");
        builder.HasKey(p => p.Id);
        builder.HasMany(p => p.Items).WithOne().HasForeignKey(i => i.IssuedPrescriptionId);
        builder.Metadata.FindNavigation(nameof(IssuedPrescription.Items))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(p => p.AppointmentId);
    }
}

internal sealed class OfflinePrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");
        builder.HasKey(i => i.Id);
    }
}

internal sealed class OfflinePrescriptionTemplateConfiguration : IEntityTypeConfiguration<PrescriptionTemplate>
{
    public void Configure(EntityTypeBuilder<PrescriptionTemplate> builder)
    {
        builder.ToTable("PrescriptionTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200);
        builder.Property(t => t.Species).HasMaxLength(100);
        builder.HasMany(t => t.Items).WithOne().HasForeignKey(i => i.PrescriptionTemplateId);
        builder.Metadata.FindNavigation(nameof(PrescriptionTemplate.Items))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class OfflinePrescriptionTemplateItemConfiguration : IEntityTypeConfiguration<PrescriptionTemplateItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionTemplateItem> builder)
    {
        builder.ToTable("PrescriptionTemplateItems");
        builder.HasKey(i => i.Id);
    }
}

internal sealed class OfflineClinicalQuoteConfiguration : IEntityTypeConfiguration<ClinicalQuote>
{
    public void Configure(EntityTypeBuilder<ClinicalQuote> builder)
    {
        builder.ToTable("ClinicalQuotes");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Notes).HasMaxLength(2000);
        builder.HasMany(q => q.Items).WithOne().HasForeignKey(i => i.ClinicalQuoteId);
        builder.Metadata.FindNavigation(nameof(ClinicalQuote.Items))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(q => q.AppointmentId);
        builder.HasIndex(q => q.PetId);
    }
}

internal sealed class OfflineClinicalQuoteItemConfiguration : IEntityTypeConfiguration<ClinicalQuoteItem>
{
    public void Configure(EntityTypeBuilder<ClinicalQuoteItem> builder)
    {
        builder.ToTable("ClinicalQuoteItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).HasMaxLength(500);
    }
}

internal sealed class OfflineClinicalAttachmentConfiguration : IEntityTypeConfiguration<ClinicalAttachment>
{
    public void Configure(EntityTypeBuilder<ClinicalAttachment> builder)
    {
        builder.ToTable("ClinicalAttachments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).HasMaxLength(260);
        builder.Property(a => a.BlobKey).HasMaxLength(500);
        builder.HasIndex(a => a.AppointmentId);
    }
}
