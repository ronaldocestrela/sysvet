using Veterinary.Application.Clinical.Dtos;
using Veterinary.Domain.Entities;

namespace Veterinary.Application.Clinical;

internal static class ClinicalMappings
{
    public static PrescriptionTemplateDto MapTemplate(PrescriptionTemplate template) =>
        new()
        {
            Id = template.Id,
            Name = template.Name,
            Species = template.Species,
            IsActive = template.IsActive,
            Items = template.Items.OrderBy(i => i.SortOrder).Select(MapLine).ToList()
        };

    public static PrescriptionLineDto MapLine(PrescriptionTemplateItem item) =>
        new()
        {
            Id = item.Id,
            MedicationName = item.MedicationName,
            Concentration = item.Concentration,
            Dose = item.Dose,
            Route = item.Route,
            Frequency = item.Frequency,
            Duration = item.Duration,
            Instructions = item.Instructions,
            SortOrder = item.SortOrder
        };

    public static PrescriptionLineDto MapLine(PrescriptionItem item) =>
        new()
        {
            Id = item.Id,
            MedicationName = item.MedicationName,
            Concentration = item.Concentration,
            Dose = item.Dose,
            Route = item.Route,
            Frequency = item.Frequency,
            Duration = item.Duration,
            Instructions = item.Instructions,
            SortOrder = item.SortOrder
        };

    public static IssuedPrescriptionDto MapPrescription(IssuedPrescription prescription) =>
        new()
        {
            Id = prescription.Id,
            AppointmentId = prescription.AppointmentId,
            PetId = prescription.PetId,
            VeterinarianId = prescription.VeterinarianId,
            TemplateId = prescription.TemplateId,
            Status = prescription.Status.ToString(),
            Items = prescription.Items.OrderBy(i => i.SortOrder).Select(MapLine).ToList()
        };

    public static ClinicalExamDto MapExam(ClinicalExam exam) =>
        new()
        {
            Id = exam.Id,
            AppointmentId = exam.AppointmentId,
            PetId = exam.PetId,
            Name = exam.Name,
            Category = exam.Category.ToString(),
            Status = exam.Status.ToString(),
            ResultSummary = exam.ResultSummary
        };

    public static ClinicalAttachmentDto MapAttachment(ClinicalAttachment attachment) =>
        new()
        {
            Id = attachment.Id,
            AppointmentId = attachment.AppointmentId,
            MedicalRecordId = attachment.MedicalRecordId,
            ClinicalExamId = attachment.ClinicalExamId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            SizeBytes = attachment.SizeBytes,
            Kind = attachment.Kind.ToString()
        };
}
