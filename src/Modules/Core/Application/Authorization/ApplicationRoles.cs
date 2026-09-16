namespace Core.Application.Authorization;

/// <summary>
/// Canonical ASP.NET Core Identity role names used by authorization policies and database seeding.
/// </summary>
public static class ApplicationRoles
{
    /// <summary>Clinic administrator with full tenant access.</summary>
    public const string Admin = "Admin";

    /// <summary>Clinical staff performing medical workflows.</summary>
    public const string Veterinarian = "Veterinarian";

    /// <summary>Front-desk staff for CRM and scheduling.</summary>
    public const string Receptionist = "Receptionist";

    /// <summary>Point-of-sale operator.</summary>
    public const string Cashier = "Cashier";

    /// <summary>All roles seeded at application startup for RBAC policies.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        Admin,
        Veterinarian,
        Receptionist,
        Cashier
    ];
}
