using Core.Domain;

namespace TutorPortal.Domain;

/// <summary>
/// Standardized error codes for the TutorPortal module.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Tutor portal account linkage errors.
    /// </summary>
    public static class Account
    {
        /// <summary>
        /// No portal account exists for the requested user or tutor.
        /// </summary>
        public static readonly Error NotFound = new("TutorPortal.Account.NotFound", "Portal account not found.");

        /// <summary>
        /// A portal account already exists for this CRM tutor or Identity user.
        /// </summary>
        public static readonly Error AlreadyLinked = new("TutorPortal.Account.AlreadyLinked", "A portal account already exists.");

        /// <summary>
        /// Self-registration could not match CRM data or is not allowed.
        /// </summary>
        public static readonly Error RegistrationDenied = new("TutorPortal.RegistrationDenied", "Registration is not available for the provided details.");

        /// <summary>
        /// Credentials are valid but the caller used the wrong login surface (clinic vs tutor portal).
        /// </summary>
        public static readonly Error WrongPortal = new("TutorPortal.WrongPortal", "Use the tutor portal to sign in with this account.");
    }

    /// <summary>
    /// Pet access errors for tutor-scoped health APIs.
    /// </summary>
    public static class Pet
    {
        /// <summary>
        /// Pet is missing, deleted, or not linked to the authenticated tutor.
        /// </summary>
        public static readonly Error NotFound = new("TutorPortal.Pet.NotFound", "Pet not found.");
    }

    /// <summary>
    /// Tutor self-service booking errors.
    /// </summary>
    public static class Booking
    {
        /// <summary>No bookable slot covers the requested interval.</summary>
        public static readonly Error SlotUnavailable = new("TutorPortal.Booking.SlotUnavailable", "The selected time is no longer available.");

        /// <summary>Professional already has an overlapping appointment.</summary>
        public static readonly Error Overlap = new("TutorPortal.Booking.Overlap", "The selected time conflicts with another appointment.");

        /// <summary>Booking or service was not found for the tutor context.</summary>
        public static readonly Error NotFound = new("TutorPortal.Booking.NotFound", "Appointment not found.");

        /// <summary>Service is missing, inactive, or not bookable by tutors.</summary>
        public static readonly Error InvalidService = new("TutorPortal.Booking.InvalidService", "The selected service is not available.");

        /// <summary>Booking kind or parameters are invalid.</summary>
        public static readonly Error InvalidRequest = new("TutorPortal.Booking.InvalidRequest", "Invalid booking request.");
    }

    /// <summary>
    /// Web Push subscription validation errors.
    /// </summary>
    public static class Push
    {
        /// <summary>Identity user is missing for push registration.</summary>
        public static readonly Error InvalidUser = new("TutorPortal.Push.InvalidUser", "Invalid user for push subscription.");

        /// <summary>Push endpoint URL is invalid.</summary>
        public static readonly Error InvalidEndpoint = new("TutorPortal.Push.InvalidEndpoint", "Invalid push endpoint.");

        /// <summary>Subscription keys are missing.</summary>
        public static readonly Error InvalidKeys = new("TutorPortal.Push.InvalidKeys", "Invalid push subscription keys.");

        /// <summary>No subscription exists for the user and endpoint.</summary>
        public static readonly Error NotFound = new("TutorPortal.Push.NotFound", "Push subscription not found.");
    }
}
