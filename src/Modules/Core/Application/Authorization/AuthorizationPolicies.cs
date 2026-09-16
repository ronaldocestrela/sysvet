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
}
