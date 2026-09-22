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
}
