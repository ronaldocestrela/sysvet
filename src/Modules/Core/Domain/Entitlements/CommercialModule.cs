namespace Core.Domain.Entitlements;

/// <summary>
/// Billable product modules enforced by Platform entitlements (roadmap 9.3).
/// Core CRM, users and access profiles are not commercial modules.
/// </summary>
public enum CommercialModule
{
    /// <summary>Clinical CRM, appointments, records, vaccines (excludes ward).</summary>
    Veterinary = 1,

    /// <summary>Inpatient ward and hospitalizations.</summary>
    Hospital = 2,

    /// <summary>Products, stock, purchases.</summary>
    Inventory = 3,

    /// <summary>Accounts payable/receivable and cash flow.</summary>
    Finance = 4,

    /// <summary>Grooming and bath services.</summary>
    Petshop = 5,

    /// <summary>NF-e / NFC-e / NFS-e emission.</summary>
    Fiscal = 6,

    /// <summary>Automated messaging and campaigns.</summary>
    Automations = 7,

    /// <summary>POS, offline sales and commissions.</summary>
    Sales = 8,

    /// <summary>Public clinic site builder.</summary>
    ClinicSite = 9,

    /// <summary>Online store and marketplace.</summary>
    Commerce = 10,

    /// <summary>Tutor self-service portal integration.</summary>
    TutorPortal = 11
}
