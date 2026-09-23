using Core.Domain;

namespace Platform.Domain;

/// <summary>
/// Platform module error codes for Result failures.
/// </summary>
public static class ErrorCodes
{
    /// <summary>Tenant catalog validation errors.</summary>
    public static class Tenant
    {
        public static readonly Error NotFound = new("Platform.Tenant.NotFound", "Tenant não encontrado.");
        public static readonly Error InvalidSlug = new("Platform.Tenant.InvalidSlug", "Slug do tenant inválido.");
        public static readonly Error DuplicateSlug = new("Platform.Tenant.DuplicateSlug", "Slug do tenant já está em uso.");
        public static readonly Error InvalidId = new("Platform.Tenant.InvalidId", "Tenant id inválido.");
        public static readonly Error InvalidDisplayName = new("Platform.Tenant.InvalidDisplayName", "Nome do tenant inválido.");
        public static readonly Error InvalidStatusTransition = new("Platform.Tenant.InvalidStatusTransition", "Transição de status inválida.");
        public static readonly Error AlreadyDeleted = new("Platform.Tenant.AlreadyDeleted", "Tenant já foi excluído.");
        public static readonly Error NotActive = new("Platform.Tenant.NotActive", "Tenant não está ativo.");
        public static readonly Error ProvisioningFailed = new("Platform.Tenant.ProvisioningFailed", "Falha ao provisionar o tenant.");
    }

    /// <summary>Branch registry validation errors.</summary>
    public static class Branch
    {
        public static readonly Error NotFound = new("Platform.Branch.NotFound", "Filial não encontrada.");
        public static readonly Error InvalidCnpj = new("Platform.Branch.InvalidCnpj", "CNPJ inválido.");
        public static readonly Error InvalidLegalName = new("Platform.Branch.InvalidLegalName", "Razão social inválida.");
        public static readonly Error InvalidTenant = new("Platform.Branch.InvalidTenant", "Tenant inválido.");
        public static readonly Error DuplicateCnpj = new("Platform.Branch.DuplicateCnpj", "CNPJ já cadastrado para este tenant.");
        public static readonly Error DuplicateHeadquarters = new("Platform.Branch.DuplicateHeadquarters", "Matriz já cadastrada para este tenant.");
        public static readonly Error AlreadyDeleted = new("Platform.Branch.AlreadyDeleted", "Filial já foi excluída.");
    }

    /// <summary>Request tenancy resolution errors.</summary>
    public static class Tenancy
    {
        public static readonly Error TenantRequired = new("Platform.Tenancy.TenantRequired", "Tenant não identificado na requisição.");
        public static readonly Error TenantMismatch = new("Platform.Tenancy.TenantMismatch", "Tenant da requisição não corresponde ao token.");
    }

    /// <summary>Plan and add-on catalog errors.</summary>
    public static class Catalog
    {
        public static readonly Error NotFound = new("Platform.Catalog.NotFound", "Item de catálogo não encontrado.");
        public static readonly Error InvalidCode = new("Platform.Catalog.InvalidCode", "Código de catálogo inválido.");
        public static readonly Error InvalidName = new("Platform.Catalog.InvalidName", "Nome de catálogo inválido.");
        public static readonly Error InvalidPrice = new("Platform.Catalog.InvalidPrice", "Preço inválido.");
        public static readonly Error DuplicateCode = new("Platform.Catalog.DuplicateCode", "Código de catálogo já existe.");
    }

    /// <summary>Tenant subscription errors.</summary>
    public static class Subscription
    {
        public static readonly Error NotFound = new("Platform.Subscription.NotFound", "Assinatura não encontrada.");
        public static readonly Error InvalidReference = new("Platform.Subscription.InvalidReference", "Referência de assinatura inválida.");
        public static readonly Error InvalidTrialDays = new("Platform.Subscription.InvalidTrialDays", "Dias de trial inválidos.");
        public static readonly Error TrialExpired = new("Platform.Subscription.TrialExpired", "Trial expirado.");
        public static readonly Error TrialNotDue = new("Platform.Subscription.TrialNotDue", "Trial ainda não venceu.");
        public static readonly Error AddOnAlreadyActive = new("Platform.Subscription.AddOnAlreadyActive", "Add-on já está ativo.");
        public static readonly Error AddOnNotActive = new("Platform.Subscription.AddOnNotActive", "Add-on não está ativo.");
    }

    /// <summary>Feature flag errors.</summary>
    public static class FeatureFlag
    {
        public static readonly Error InvalidTenant = new("Platform.FeatureFlag.InvalidTenant", "Tenant inválido para feature flag.");
    }

    /// <summary>Commercial module entitlement errors.</summary>
    public static class Entitlement
    {
        public static readonly Error ModuleDisabled = new("Platform.Entitlement.ModuleDisabled", "Módulo não disponível no plano atual.");
    }
}
