using Core.Domain.Entitlements;

namespace API.Routing;

/// <summary>
/// Maps API route prefixes to commercial modules for entitlement enforcement (9.3).
/// </summary>
public static class CommercialModuleRouteMapper
{
    /// <summary>Path prefixes exempt from commercial module checks (core staff APIs).</summary>
    public static bool IsCoreExempt(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.StartsWith("/api/v1/users", StringComparison.OrdinalIgnoreCase)
               || value.StartsWith("/api/v1/access-profiles", StringComparison.OrdinalIgnoreCase)
               || value.StartsWith("/api/v1/me/", StringComparison.OrdinalIgnoreCase)
               || value.StartsWith("/api/v1/audit", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Resolves required module for a path, or null when route is not commercial.</summary>
    public static CommercialModule? ResolveModule(PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (!value.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (IsCoreExempt(path))
        {
            return null;
        }

        if (value.StartsWith("/api/v1/grooming", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Petshop;
        }

        if (value.StartsWith("/api/v1/fiscal", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Fiscal;
        }

        if (value.StartsWith("/api/v1/automations", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Automations;
        }

        if (value.StartsWith("/api/v1/sales", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Sales;
        }

        if (value.StartsWith("/api/v1/hospitalizations", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/ward-units", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Hospital;
        }

        if (value.StartsWith("/api/v1/finance", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/financial-", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/cost-centers", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Finance;
        }

        if (value.StartsWith("/api/v1/products", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/stock", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/suppliers", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/purchase-", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/inventory", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Inventory;
        }

        if (value.StartsWith("/api/v1/clinic-site", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.ClinicSite;
        }

        if (value.StartsWith("/api/v1/commerce", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Commerce;
        }

        if (value.StartsWith("/api/v1/tutors", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/pets", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/appointments", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/schedule-slots", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/medical-records", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/vaccine-protocols", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/prescription", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/exams", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/attachments", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/clinical-quotes", StringComparison.OrdinalIgnoreCase))
        {
            return CommercialModule.Veterinary;
        }

        return null;
    }
}
