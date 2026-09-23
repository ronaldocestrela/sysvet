namespace Core.Domain.Authorization;

/// <summary>
/// Static catalog of fine-grained permissions; admins compose profiles from this set only.
/// </summary>
public static class Permissions
{
    public const string UsersRead = "Users.Read";
    public const string UsersWrite = "Users.Write";
    public const string UsersDisable = "Users.Disable";

    public const string ProfilesRead = "Profiles.Read";
    public const string ProfilesWrite = "Profiles.Write";

    public const string TutorsRead = "Tutors.Read";
    public const string TutorsWrite = "Tutors.Write";
    public const string TutorsDelete = "Tutors.Delete";

    public const string PetsRead = "Pets.Read";
    public const string PetsWrite = "Pets.Write";
    public const string PetsDelete = "Pets.Delete";

    public const string AppointmentsRead = "Appointments.Read";
    public const string AppointmentsWrite = "Appointments.Write";

    public const string GroomingRead = "Grooming.Read";
    public const string GroomingWrite = "Grooming.Write";

    public const string MedicalRecordsRead = "MedicalRecords.Read";
    public const string MedicalRecordsWrite = "MedicalRecords.Write";

    public const string HospitalizationsRead = "Hospitalizations.Read";
    public const string HospitalizationsWrite = "Hospitalizations.Write";

    public const string VaccinesRead = "Vaccines.Read";
    public const string VaccinesWrite = "Vaccines.Write";

    public const string ClinicalQuotesRead = "ClinicalQuotes.Read";
    public const string ClinicalQuotesWrite = "ClinicalQuotes.Write";

    public const string ProductsRead = "Products.Read";
    public const string ProductsWrite = "Products.Write";

    public const string StockRead = "Stock.Read";
    public const string StockWrite = "Stock.Write";

    public const string PurchaseImportsRead = "PurchaseImports.Read";
    public const string PurchaseImportsWrite = "PurchaseImports.Write";

    public const string SalesRead = "Sales.Read";
    public const string SalesWrite = "Sales.Write";

    public const string CashRegisterRead = "CashRegister.Read";
    public const string CashRegisterWrite = "CashRegister.Write";

    public const string FinanceRead = "Finance.Read";
    public const string FinanceWrite = "Finance.Write";

    public const string FiscalRead = "Fiscal.Read";
    public const string FiscalWrite = "Fiscal.Write";

    public const string AutomationsRead = "Automations.Read";
    public const string AutomationsWrite = "Automations.Write";

    public const string AuditRead = "Audit.Read";

    public const string PrivacyExport = "Privacy.Export";
    public const string PrivacyErase = "Privacy.Erase";

    public const string ClinicSiteRead = "ClinicSite.Read";
    public const string ClinicSiteWrite = "ClinicSite.Write";

    public const string CommerceRead = "Commerce.Read";
    public const string CommerceWrite = "Commerce.Write";

    public const string IntelligenceRead = "Intelligence.Read";
    public const string IntelligenceLayoutWrite = "Intelligence.LayoutWrite";

    /// <summary>
    /// All defined permission codes in stable order.
    /// </summary>
    public static readonly IReadOnlyList<string> All =
    [
        UsersRead, UsersWrite, UsersDisable,
        ProfilesRead, ProfilesWrite,
        TutorsRead, TutorsWrite, TutorsDelete,
        PetsRead, PetsWrite, PetsDelete,
        AppointmentsRead, AppointmentsWrite,
        GroomingRead, GroomingWrite,
        MedicalRecordsRead, MedicalRecordsWrite,
        HospitalizationsRead, HospitalizationsWrite,
        VaccinesRead, VaccinesWrite,
        ClinicalQuotesRead, ClinicalQuotesWrite,
        ProductsRead, ProductsWrite,
        StockRead, StockWrite,
        PurchaseImportsRead, PurchaseImportsWrite,
        SalesRead, SalesWrite,
        CashRegisterRead, CashRegisterWrite,
        FinanceRead, FinanceWrite,
        FiscalRead, FiscalWrite,
        AutomationsRead, AutomationsWrite,
        AuditRead,
        PrivacyExport, PrivacyErase,
        ClinicSiteRead, ClinicSiteWrite,
        CommerceRead, CommerceWrite,
        IntelligenceRead, IntelligenceLayoutWrite
    ];

    /// <summary>
    /// Returns whether the string matches a catalog entry.
    /// </summary>
    public static bool IsValid(string code) => All.Contains(code, StringComparer.Ordinal);

    /// <summary>
    /// Default grants for the Admin system profile (full catalog).
    /// </summary>
    public static IReadOnlyList<string> AdminDefaults() => All;

    /// <summary>
    /// Default grants aligned with Veterinarian policy (clinical + CRM, no user/profile admin).
    /// </summary>
    public static IReadOnlyList<string> VeterinarianDefaults() =>
    [
        TutorsRead, TutorsWrite, TutorsDelete,
        PetsRead, PetsWrite, PetsDelete,
        AppointmentsRead, AppointmentsWrite,
        GroomingRead, GroomingWrite,
        MedicalRecordsRead, MedicalRecordsWrite,
        HospitalizationsRead, HospitalizationsWrite,
        VaccinesRead, VaccinesWrite,
        ClinicalQuotesRead, ClinicalQuotesWrite,
        ProductsRead,
        IntelligenceRead
    ];

    /// <summary>
    /// Default grants aligned with Receptionist policy (CRM + scheduling, no delete/admin/cash).
    /// </summary>
    public static IReadOnlyList<string> ReceptionistDefaults() =>
    [
        TutorsRead, TutorsWrite,
        PetsRead, PetsWrite,
        AppointmentsRead, AppointmentsWrite,
        GroomingRead, GroomingWrite,
        HospitalizationsRead,
        VaccinesRead,
        ClinicalQuotesRead, ClinicalQuotesWrite,
        ProductsRead, ProductsWrite,
        StockRead,
        PurchaseImportsRead, PurchaseImportsWrite,
        FinanceRead, FinanceWrite,
        FiscalRead, FiscalWrite,
        AutomationsRead, AutomationsWrite,
        ClinicSiteRead, ClinicSiteWrite,
        CommerceRead, CommerceWrite,
        IntelligenceRead
    ];

    /// <summary>
    /// Default grants aligned with Cashier policy (sales only).
    /// </summary>
    public static IReadOnlyList<string> CashierDefaults() =>
    [
        SalesRead, SalesWrite,
        CashRegisterRead, CashRegisterWrite,
        ProductsRead,
        StockRead,
        ClinicalQuotesRead,
        FiscalRead, FiscalWrite,
        IntelligenceRead
    ];
}
