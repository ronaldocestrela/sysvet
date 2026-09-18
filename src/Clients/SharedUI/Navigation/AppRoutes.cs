namespace SharedUI.Navigation;

/// <summary>
/// Central route paths for client navigation to avoid magic strings across layouts and pages.
/// </summary>
public static class AppRoutes
{
    /// <summary>Application home / dashboard.</summary>
    public const string Home = "/";

    /// <summary>Staff login.</summary>
    public const string Login = "/login";

    /// <summary>CRM tutors list.</summary>
    public const string Tutors = "/tutors";

    /// <summary>CRM pets list.</summary>
    public const string Pets = "/pets";

    /// <summary>Pet clinical medical record timeline.</summary>
    public static string PetMedicalRecords(Guid petId, Guid? appointmentId = null) =>
        appointmentId is null
            ? $"/pets/{petId}/medical-records"
            : $"/pets/{petId}/medical-records?appointmentId={appointmentId}";

    /// <summary>Pet vaccination card (printable).</summary>
    public static string PetVaccinationCard(Guid petId) => $"/pets/{petId}/vaccination-card";

    /// <summary>Clinic vaccine alerts backoffice list.</summary>
    public const string VaccineAlerts = "/vaccine-alerts";

    /// <summary>Approved quotes pending PDV conversion.</summary>
    public const string PendingQuoteConversions = "/clinical-quotes/pending";

    /// <summary>Printable clinical quote.</summary>
    public static string PetClinicalQuotePrint(Guid petId, Guid quoteId) => $"/pets/{petId}/quotes/{quoteId}";

    /// <summary>Clinical appointments.</summary>
    public const string Appointments = "/appointments";

    /// <summary>Hospitalizations.</summary>
    public const string Hospitalizations = "/hospitalizations";

    /// <summary>Ward units and beds configuration.</summary>
    public const string HospitalizationUnits = "/hospitalizations/units";

    /// <summary>Hospitalization detail.</summary>
    public static string HospitalizationDetail(Guid id) => $"/hospitalizations/{id}";

    /// <summary>Inventory products.</summary>
    public const string Products = "/products";

    /// <summary>Stock movements.</summary>
    public const string StockMovements = "/stock-movements";

    /// <summary>Point of sale terminal.</summary>
    public const string SalesPos = "/sales/pos";

    /// <summary>Cash register.</summary>
    public const string SalesCashRegister = "/sales/cash-register";
}
