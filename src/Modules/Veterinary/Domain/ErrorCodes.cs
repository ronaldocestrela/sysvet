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
        public static readonly Error Finalized = new("MedicalRecord.Finalized", "Cannot modify a finalized medical record.");
        public static readonly Error AlreadyFinalized = new("MedicalRecord.AlreadyFinalized", "The medical record is already finalized.");
    }

    public static class VaccineDose
    {
        public static readonly Error InvalidName = new("VaccineDose.InvalidName", "Vaccine name cannot be empty.");
        public static readonly Error FutureApplicationDate = new("VaccineDose.FutureApplicationDate", "Application date cannot be in the future.");
    }

    public static class PrescriptionExecution
    {
        public static readonly Error InvalidMedicationName = new("PrescriptionExecution.InvalidMedicationName", "Medication name cannot be empty.");
    }
}
