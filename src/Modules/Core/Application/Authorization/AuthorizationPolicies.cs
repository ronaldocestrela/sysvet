namespace Core.Application.Authorization;

/// <summary>
/// Named authorization policies registered in Core infrastructure and referenced by <see cref="Behaviors.AuthorizeRequestAttribute"/>.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Any authenticated user.
    /// </summary>
    public const string Authenticated = "Authenticated";

    /// <summary>
    /// Clinic staff roles: Admin, Veterinarian, Receptionist.
    /// </summary>
    public const string ClinicStaff = "ClinicStaff";

    public const string Admin = "Admin";
    public const string Veterinarian = "Veterinarian";
    public const string Receptionist = "Receptionist";
    public const string Cashier = "Cashier";

    /// <summary>
    /// Any clinic staff or backoffice role (excludes tutor portal users).
    /// </summary>
    public const string ClinicUser = "ClinicUser";

    /// <summary>
    /// Tutor portal users linked to a CRM tutor record.
    /// </summary>
    public const string TutorPortal = "TutorPortal";

    /// <summary>
    /// VetNexus Super Admin operators managing tenants and billing.
    /// </summary>
    public const string PlatformAdmin = "PlatformAdmin";
}
