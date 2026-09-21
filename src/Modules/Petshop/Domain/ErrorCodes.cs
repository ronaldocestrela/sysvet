using Core.Domain;

namespace Petshop.Domain;

/// <summary>
/// Standardized error codes for the Petshop module.
/// </summary>
public static class ErrorCodes
{
    public static class GroomingAppointment
    {
        public static readonly Error NotFound = new("GroomingAppointment.NotFound", "The specified grooming appointment was not found.");
        public static readonly Error InvalidDate = new("GroomingAppointment.InvalidDate", "The appointment date cannot be in the past.");
        public static readonly Error InvalidStatusTransition = new("GroomingAppointment.InvalidStatusTransition", "The appointment cannot transition to this status.");
        public static readonly Error SlotUnavailable = new("GroomingAppointment.SlotUnavailable", "The requested time slot is not available.");
        public static readonly Error Overlap = new("GroomingAppointment.Overlap", "Another appointment overlaps this time for the groomer.");
    }

    public static class GroomingSlot
    {
        public static readonly Error NotFound = new("GroomingSlot.NotFound", "The specified grooming slot was not found.");
        public static readonly Error NotAvailable = new("GroomingSlot.NotAvailable", "The grooming slot is not available.");
    }

    public static class GroomingRecord
    {
        public static readonly Error NotFound = new("GroomingRecord.NotFound", "The specified grooming record was not found.");
        public static readonly Error Finalized = new("GroomingRecord.Finalized", "Cannot modify a finalized grooming record.");
        public static readonly Error AlreadyFinalized = new("GroomingRecord.AlreadyFinalized", "The grooming record is already finalized.");
        public static readonly Error InvalidIdentifiers = new("GroomingRecord.InvalidIdentifiers", "Grooming record requires valid appointment, groomer, tutor, and pet identifiers.");
        public static readonly Error InvalidSupply = new("GroomingRecord.InvalidSupply", "Supply lines require a valid product and positive quantity.");
        public static readonly Error InvalidCoatNotes = new("GroomingRecord.InvalidCoatNotes", "Coat notes exceed maximum length.");
    }

    public static class GroomingService
    {
        public static readonly Error NotFound = new("GroomingService.NotFound", "The specified grooming service was not found.");
        public static readonly Error InvalidName = new("GroomingService.InvalidName", "Service name is required and must be at most 150 characters.");
        public static readonly Error InvalidDuration = new("GroomingService.InvalidDuration", "Duration must be greater than zero.");
        public static readonly Error InvalidSupply = new("GroomingService.InvalidSupply", "Default supplies require a valid product and positive quantity.");
    }
}
