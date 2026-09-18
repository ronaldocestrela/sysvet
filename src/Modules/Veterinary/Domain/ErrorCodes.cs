using Core.Domain;

namespace Veterinary.Domain;

/// <summary>
/// Standardized error codes for the Veterinary module.
/// </summary>
public static class ErrorCodes
{
    public static class Appointment
    {
        public static readonly Error NotFound = new("Appointment.NotFound", "The specified appointment was not found.");
        public static readonly Error SlotUnavailable = new("Appointment.SlotUnavailable", "The requested time slot is not available.");
        public static readonly Error Overlap = new("Appointment.Overlap", "Another appointment overlaps this time for the veterinarian.");
    }

    public static class ScheduleSlot
    {
        public static readonly Error NotFound = new("ScheduleSlot.NotFound", "The specified schedule slot was not found.");
        public static readonly Error NotAvailable = new("ScheduleSlot.NotAvailable", "The schedule slot is not available.");
    }

    public static class Hospitalization
    {
        public static readonly Error NotFound = new("Hospitalization.NotFound", "The specified hospitalization was not found.");
        public static readonly Error InvalidReason = new("Hospitalization.InvalidReason", "Reason for admission cannot be empty.");
        public static readonly Error AlreadyDischarged = new("Hospitalization.AlreadyDischarged", "The hospitalization is already discharged.");
        public static readonly Error Discharged = new("Hospitalization.Discharged", "Cannot execute prescriptions for a discharged patient.");
    }

    public static class MedicalRecord
    {
        public static readonly Error NotFound = new("MedicalRecord.NotFound", "The specified medical record was not found.");
        public static readonly Error Finalized = new("MedicalRecord.Finalized", "Cannot modify a finalized medical record.");
        public static readonly Error AlreadyFinalized = new("MedicalRecord.AlreadyFinalized", "The medical record is already finalized.");
        public static readonly Error AppointmentNotEligible = new("MedicalRecord.AppointmentNotEligible", "Medical records can only be opened for in-progress or completed appointments.");
        public static readonly Error EmptyEvolution = new("MedicalRecord.EmptyEvolution", "Evolution note text and author are required.");
        public static readonly Error InvalidEvolution = new("MedicalRecord.InvalidEvolution", "Evolution note exceeds maximum length.");
        public static readonly Error InvalidVitals = new("MedicalRecord.InvalidVitals", "Vital signs values are out of acceptable range.");
        public static readonly Error InvalidIdentifiers = new("MedicalRecord.InvalidIdentifiers", "Medical record requires valid appointment, veterinarian, tutor, and pet identifiers.");
        public static readonly Error InvalidAnamnesis = new("MedicalRecord.InvalidAnamnesis", "Anamnesis exceeds maximum length.");
        public static readonly Error InvalidDiagnosis = new("MedicalRecord.InvalidDiagnosis", "Diagnosis exceeds maximum length.");
        public static readonly Error InvalidConduct = new("MedicalRecord.InvalidConduct", "Conduct exceeds maximum length.");
    }

    public static class VaccineDose
    {
        public static readonly Error NotFound = new("VaccineDose.NotFound", "The specified vaccine dose was not found.");
        public static readonly Error InvalidName = new("VaccineDose.InvalidName", "Vaccine name cannot be empty.");
        public static readonly Error FutureApplicationDate = new("VaccineDose.FutureApplicationDate", "Application date cannot be in the future.");
        public static readonly Error SpeciesMismatch = new("VaccineDose.SpeciesMismatch", "The vaccine protocol does not match the pet species.");
        public static readonly Error InvalidProtocolDose = new("VaccineDose.InvalidProtocolDose", "The specified protocol dose was not found.");
    }

    public static class VaccineProtocol
    {
        public static readonly Error NotFound = new("VaccineProtocol.NotFound", "The specified vaccine protocol was not found.");
        public static readonly Error InvalidName = new("VaccineProtocol.InvalidName", "Protocol name is required and must be at most 200 characters.");
        public static readonly Error InvalidSpecies = new("VaccineProtocol.InvalidSpecies", "Protocol species must be valid.");
        public static readonly Error InvalidIdentifiers = new("VaccineProtocol.InvalidIdentifiers", "Protocol identifiers are invalid.");
        public static readonly Error InvalidSequence = new("VaccineProtocol.InvalidSequence", "Dose sequence must be greater than zero.");
        public static readonly Error InvalidDoseLabel = new("VaccineProtocol.InvalidDoseLabel", "Dose label is required and must be at most 100 characters.");
        public static readonly Error InvalidAgeRange = new("VaccineProtocol.InvalidAgeRange", "Dose age range is invalid.");
        public static readonly Error AlreadyInactive = new("VaccineProtocol.AlreadyInactive", "The protocol is already inactive.");
    }

    public static class PrescriptionExecution
    {
        public static readonly Error InvalidMedicationName = new("PrescriptionExecution.InvalidMedicationName", "Medication name cannot be empty.");
    }

    public static class PrescriptionTemplate
    {
        public static readonly Error NotFound = new("PrescriptionTemplate.NotFound", "The specified prescription template was not found.");
        public static readonly Error InvalidName = new("PrescriptionTemplate.InvalidName", "Template name is required and must be at most 200 characters.");
        public static readonly Error InvalidIdentifiers = new("PrescriptionTemplate.InvalidIdentifiers", "Template identifiers are invalid.");
        public static readonly Error InvalidMedication = new("PrescriptionTemplate.InvalidMedication", "Each template item requires a medication name.");
        public static readonly Error AlreadyInactive = new("PrescriptionTemplate.AlreadyInactive", "The template is already inactive.");
    }

    public static class IssuedPrescription
    {
        public static readonly Error NotFound = new("IssuedPrescription.NotFound", "The specified issued prescription was not found.");
        public static readonly Error InvalidIdentifiers = new("IssuedPrescription.InvalidIdentifiers", "Prescription requires valid appointment, pet, and veterinarian identifiers.");
        public static readonly Error AlreadyIssued = new("IssuedPrescription.AlreadyIssued", "Cannot modify an issued prescription.");
        public static readonly Error EmptyItems = new("IssuedPrescription.EmptyItems", "At least one medication line is required to issue a prescription.");
    }

    public static class ClinicalExam
    {
        public static readonly Error NotFound = new("ClinicalExam.NotFound", "The specified clinical exam was not found.");
        public static readonly Error InvalidIdentifiers = new("ClinicalExam.InvalidIdentifiers", "Exam requires valid appointment and pet identifiers.");
        public static readonly Error InvalidName = new("ClinicalExam.InvalidName", "Exam name is required and must be at most 200 characters.");
        public static readonly Error InvalidTransition = new("ClinicalExam.InvalidTransition", "The exam cannot transition to the requested status.");
        public static readonly Error InvalidResult = new("ClinicalExam.InvalidResult", "Result summary exceeds maximum length.");
        public static readonly Error AppointmentNotEligible = new("ClinicalExam.AppointmentNotEligible", "Exams can only be registered for in-progress or completed appointments.");
    }

    public static class ClinicalAttachment
    {
        public static readonly Error NotFound = new("ClinicalAttachment.NotFound", "The specified clinical attachment was not found.");
        public static readonly Error InvalidIdentifiers = new("ClinicalAttachment.InvalidIdentifiers", "Attachment requires a valid appointment identifier.");
        public static readonly Error InvalidBlobKey = new("ClinicalAttachment.InvalidBlobKey", "Blob storage key is required.");
        public static readonly Error InvalidFileName = new("ClinicalAttachment.InvalidFileName", "File name is required.");
        public static readonly Error InvalidSize = new("ClinicalAttachment.InvalidSize", "File size must be greater than zero.");
        public static readonly Error FileTooLarge = new("ClinicalAttachment.FileTooLarge", "File exceeds the maximum allowed size for its type.");
        public static readonly Error UnsupportedContentType = new("ClinicalAttachment.UnsupportedContentType", "Content type is not allowed for clinical attachments.");
        public static readonly Error AlreadyDeleted = new("ClinicalAttachment.AlreadyDeleted", "The attachment is already deleted.");
        public static readonly Error AppointmentNotEligible = new("ClinicalAttachment.AppointmentNotEligible", "Attachments can only be added for in-progress or completed appointments.");
    }
}
