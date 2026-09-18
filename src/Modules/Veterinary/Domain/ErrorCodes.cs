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
        public static readonly Error InvalidIdentifiers = new("Hospitalization.InvalidIdentifiers", "Hospitalization requires valid pet, veterinarian, and bed identifiers.");
        public static readonly Error InvalidBed = new("Hospitalization.InvalidBed", "Target bed is invalid.");
        public static readonly Error AlreadyDischarged = new("Hospitalization.AlreadyDischarged", "The hospitalization is already discharged.");
        public static readonly Error Discharged = new("Hospitalization.Discharged", "Cannot modify a discharged hospitalization.");
        public static readonly Error PetAlreadyAdmitted = new("Hospitalization.PetAlreadyAdmitted", "The pet already has an active hospitalization.");
        public static readonly Error BedOccupied = new("Hospitalization.BedOccupied", "The bed is already occupied.");
    }

    public static class WardUnit
    {
        public static readonly Error NotFound = new("WardUnit.NotFound", "The specified ward unit was not found.");
        public static readonly Error InvalidName = new("WardUnit.InvalidName", "Ward unit name is required and must be at most 200 characters.");
        public static readonly Error InvalidIdentifiers = new("WardUnit.InvalidIdentifiers", "Ward unit identifiers are invalid.");
        public static readonly Error InvalidBedCode = new("WardUnit.InvalidBedCode", "Bed code is required and must be at most 20 characters.");
        public static readonly Error AlreadyInactive = new("WardUnit.AlreadyInactive", "The ward unit is already inactive.");
    }

    public static class HospitalMedicationOrder
    {
        public static readonly Error InvalidMedication = new("HospitalMedicationOrder.InvalidMedication", "Medication name is required.");
        public static readonly Error InvalidSchedule = new("HospitalMedicationOrder.InvalidSchedule", "Medication schedule is invalid or exceeds the maximum duration.");
    }

    public static class MedicationAdministration
    {
        public static readonly Error NotFound = new("MedicationAdministration.NotFound", "The specified administration slot was not found.");
        public static readonly Error InvalidTransition = new("MedicationAdministration.InvalidTransition", "The administration slot cannot transition to the requested status.");
        public static readonly Error InvalidActor = new("MedicationAdministration.InvalidActor", "Actor identifier is required.");
    }

    public static class HospitalizationProgressNote
    {
        public static readonly Error EmptyText = new("HospitalizationProgressNote.EmptyText", "Progress note text and author are required.");
        public static readonly Error InvalidText = new("HospitalizationProgressNote.InvalidText", "Progress note exceeds maximum length.");
    }

    public static class HospitalProcedure
    {
        public static readonly Error InvalidName = new("HospitalProcedure.InvalidName", "Procedure name is required and must be at most 200 characters.");
        public static readonly Error InvalidIdentifiers = new("HospitalProcedure.InvalidIdentifiers", "Procedure requires a valid veterinarian identifier.");
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

    public static class ClinicalQuote
    {
        public static readonly Error NotFound = new("ClinicalQuote.NotFound", "The specified clinical quote was not found.");
        public static readonly Error InvalidIdentifiers = new("ClinicalQuote.InvalidIdentifiers", "Quote requires valid appointment, pet, tutor, and creator identifiers.");
        public static readonly Error AppointmentNotEligible = new("ClinicalQuote.AppointmentNotEligible", "Quotes can only be created for in-progress or completed appointments.");
        public static readonly Error EmptyItems = new("ClinicalQuote.EmptyItems", "At least one line item is required to send a quote.");
        public static readonly Error InvalidTransition = new("ClinicalQuote.InvalidTransition", "The quote cannot transition to the requested status.");
        public static readonly Error InvalidPrice = new("ClinicalQuote.InvalidPrice", "Unit price cannot be negative.");
        public static readonly Error InvalidQuantity = new("ClinicalQuote.InvalidQuantity", "Quantity must be greater than zero.");
        public static readonly Error InvalidDescription = new("ClinicalQuote.InvalidDescription", "Line description is required and must be at most 500 characters.");
        public static readonly Error AlreadyConverted = new("ClinicalQuote.AlreadyConverted", "The quote has already been converted to a sale.");
        public static readonly Error InvalidNotes = new("ClinicalQuote.InvalidNotes", "Notes exceed maximum length.");
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
