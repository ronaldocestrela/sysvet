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

    /// <summary>Grooming salon agenda.</summary>
    public const string Grooming = "/grooming";

    /// <summary>Pet grooming service history.</summary>
    public static string PetGroomingHistory(Guid petId) => $"/pets/{petId}/grooming-history";

    /// <summary>Hospitalizations.</summary>
    public const string Hospitalizations = "/hospitalizations";

    /// <summary>Ward units and beds configuration.</summary>
    public const string HospitalizationUnits = "/hospitalizations/units";

    /// <summary>Hospitalization detail.</summary>
    public static string HospitalizationDetail(Guid id) => $"/hospitalizations/{id}";

    /// <summary>Inventory products.</summary>
    public const string Products = "/products";

    /// <summary>Inventory suppliers.</summary>
    public const string Suppliers = "/suppliers";

    /// <summary>Stock movements.</summary>
    public const string StockMovements = "/stock-movements";

    /// <summary>Inventory low-stock and expiry alerts.</summary>
    public const string StockAlerts = "/stock-alerts";

    /// <summary>NF-e purchase XML import.</summary>
    public const string PurchaseImports = "/purchase-imports";

    /// <summary>Physical inventory count sessions (online).</summary>
    public const string InventoryCounts = "/inventory-counts";

    /// <summary>Purchase suggestions grouped by supplier.</summary>
    public const string PurchaseSuggestions = "/purchase-suggestions";

    /// <summary>Accounts payable and receivable.</summary>
    public const string Finance = "/finance";

    /// <summary>Card TEF reconciliation (online).</summary>
    public const string FinanceCardReconciliation = "/finance/card-reconciliation";

    /// <summary>Cash flow and simplified income statement reports.</summary>
    public const string FinanceReports = "/finance/reports";

    /// <summary>Point of sale terminal.</summary>
    public const string SalesPos = "/sales/pos";

    /// <summary>Cash register.</summary>
    public const string SalesCashRegister = "/sales/cash-register";

    /// <summary>Commission rule configuration (online).</summary>
    public const string SalesCommissionRules = "/sales/commission-rules";

    /// <summary>Prepaid service package balances and consumption.</summary>
    public const string SalesPrepaidBalances = "/sales/prepaid-balances";

    /// <summary>Automations, campaigns and message jobs.</summary>
    public const string Automations = "/automations";

    /// <summary>Public clinic site configuration.</summary>
    public const string ClinicSite = "/clinic-site";

    /// <summary>Commerce product offers and pricing.</summary>
    public const string CommerceOffers = "/commerce/offers";

    /// <summary>Online order fulfillment.</summary>
    public const string CommerceOrders = "/commerce/orders";

    /// <summary>Public NPS survey (tokenized, no login).</summary>
    public static string NpsSurvey(string token) => $"/nps/{token}";

    public const string Fiscal = "/fiscal";

    /// <summary>Issuer profile and A1 certificate.</summary>
    public const string FiscalIssuer = "/fiscal/issuer";

    /// <summary>Tax planning reports and regime simulation.</summary>
    public const string FiscalPlanning = "/fiscal/planning";

    /// <summary>Fiscal document detail.</summary>
    public static string FiscalDocumentDetail(Guid id) => $"/fiscal/{id}";

    /// <summary>SaaS subscription payment when operationally locked (9.5).</summary>
    public const string BillingPayment = "/billing/payment";

    /// <summary>Dashboard widget layout editor (10.1).</summary>
    public const string IntelligenceLayouts = "/intelligence/layouts";
}
