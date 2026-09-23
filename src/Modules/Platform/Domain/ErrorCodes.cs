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

    /// <summary>SaaS billing gateway errors (9.4).</summary>
    public static class Billing
    {
        public static readonly Error InvalidTenant = new("Platform.Billing.InvalidTenant", "Tenant inválido para cobrança.");
        public static readonly Error InvalidCustomer = new("Platform.Billing.InvalidCustomer", "Dados de cliente de cobrança inválidos.");
        public static readonly Error InvalidPaymentMethod = new("Platform.Billing.InvalidPaymentMethod", "Forma de pagamento inválida.");
        public static readonly Error InvalidAmount = new("Platform.Billing.InvalidAmount", "Valor de fatura inválido.");
        public static readonly Error InvalidInvoiceTransition = new("Platform.Billing.InvalidInvoiceTransition", "Transição de fatura inválida.");
        public static readonly Error CustomerNotFound = new("Platform.Billing.CustomerNotFound", "Cliente de cobrança não cadastrado.");
        public static readonly Error PaymentMethodNotFound = new("Platform.Billing.PaymentMethodNotFound", "Forma de pagamento não cadastrada.");
        public static readonly Error NotDue = new("Platform.Billing.NotDue", "Assinatura ainda não venceu para cobrança.");
        public static readonly Error NotBillable = new("Platform.Billing.NotBillable", "Assinatura não elegível para cobrança.");
        public static readonly Error OpenInvoiceExists = new("Platform.Billing.OpenInvoiceExists", "Já existe fatura em aberto.");
        public static readonly Error InvoiceNotFound = new("Platform.Billing.InvoiceNotFound", "Fatura não encontrada.");
        public static readonly Error GatewayFailed = new("Platform.Billing.GatewayFailed", "Falha na integração com gateway de pagamento.");
        public static readonly Error WebhookUnauthorized = new("Platform.Billing.WebhookUnauthorized", "Webhook não autorizado.");
        public static readonly Error WebhookInvalidPayload = new("Platform.Billing.WebhookInvalidPayload", "Payload de webhook inválido.");
        public static readonly Error OperationalLocked = new("Platform.Billing.OperationalLocked", "Acesso operacional bloqueado por inadimplência. Regularize o pagamento.");
        public static readonly Error NoOutstandingInvoice = new("Platform.Billing.NoOutstandingInvoice", "Não há fatura pendente para pagamento.");
    }

    /// <summary>Dunning pipeline errors (9.5).</summary>
    public static class Dunning
    {
        public static readonly Error InvalidStep = new("Platform.Dunning.InvalidStep", "Passo de régua inválido.");
    }

    /// <summary>SaaS NFS-e emission errors (9.6).</summary>
    public static class Nfse
    {
        public static readonly Error InvalidAmount = new("Platform.Nfse.InvalidAmount", "Valor inválido para NFS-e SaaS.");
        public static readonly Error InvalidRecipient = new("Platform.Nfse.InvalidRecipient", "Tomador inválido para NFS-e SaaS.");
        public static readonly Error InvalidTransition = new("Platform.Nfse.InvalidTransition", "Transição de NFS-e SaaS inválida.");
        public static readonly Error InvalidGatewayResponse = new("Platform.Nfse.InvalidGatewayResponse", "Resposta inválida do gateway NFS-e.");
        public static readonly Error AlreadyAuthorized = new("Platform.Nfse.AlreadyAuthorized", "NFS-e já autorizada para esta fatura.");
        public static readonly Error HeadquartersMissing = new("Platform.Nfse.HeadquartersMissing", "Matriz (CNPJ) não cadastrada para o tenant.");
        public static readonly Error InvoiceNotPaid = new("Platform.Nfse.InvoiceNotPaid", "Fatura ainda não liquidada.");
        public static readonly Error RecipientAddressRequired = new("Platform.Nfse.RecipientAddressRequired", "Endereço completo da matriz é obrigatório para NFS-e OpenAC.");
        public static readonly Error IssuerNotConfigured = new("Platform.Nfse.IssuerNotConfigured", "Emissor VetNexus não configurado.");
    }

    /// <summary>Impersonation errors (9.6).</summary>
    public static class Impersonation
    {
        public static readonly Error InvalidActor = new("Platform.Impersonation.InvalidActor", "Operador de suporte inválido.");
        public static readonly Error InvalidTarget = new("Platform.Impersonation.InvalidTarget", "Tenant alvo inválido.");
        public static readonly Error InvalidDuration = new("Platform.Impersonation.InvalidDuration", "Duração de sessão inválida.");
        public static readonly Error SessionNotFound = new("Platform.Impersonation.SessionNotFound", "Sessão de impersonation não encontrada.");
        public static readonly Error SessionExpired = new("Platform.Impersonation.SessionExpired", "Sessão de impersonation expirada ou encerrada.");
        public static readonly Error AlreadyEnded = new("Platform.Impersonation.AlreadyEnded", "Sessão já encerrada.");
        public static readonly Error TenantNotAllowed = new("Platform.Impersonation.TenantNotAllowed", "Tenant não elegível para impersonation.");
        public static readonly Error Forbidden = new("Platform.Impersonation.Forbidden", "Operação não permitida para esta sessão.");
    }

    /// <summary>Platform audit and login log errors (9.7).</summary>
    public static class Audit
    {
        public static readonly Error InvalidEmail = new("Platform.Audit.InvalidEmail", "E-mail de login inválido.");
        public static readonly Error InvalidActor = new("Platform.Audit.InvalidActor", "Operador de auditoria inválido.");
        public static readonly Error InvalidAction = new("Platform.Audit.InvalidAction", "Ação de auditoria inválida.");
    }

    /// <summary>Partner API key errors (9.7).</summary>
    public static class ApiKey
    {
        public static readonly Error InvalidTenant = new("Platform.ApiKey.InvalidTenant", "Tenant inválido para API key.");
        public static readonly Error InvalidPartnerName = new("Platform.ApiKey.InvalidPartnerName", "Nome do parceiro inválido.");
        public static readonly Error InvalidSecret = new("Platform.ApiKey.InvalidSecret", "Segredo de API key inválido.");
        public static readonly Error NotFound = new("Platform.ApiKey.NotFound", "API key não encontrada.");
        public static readonly Error AlreadyRevoked = new("Platform.ApiKey.AlreadyRevoked", "API key já revogada.");
        public static readonly Error MissingHeader = new("Platform.ApiKey.MissingHeader", "Cabeçalho X-Api-Key ausente.");
        public static readonly Error Invalid = new("Platform.ApiKey.Invalid", "API key inválida ou revogada.");
    }

    /// <summary>Tenant health metrics errors (9.7).</summary>
    public static class Health
    {
        public static readonly Error InvalidTenant = new("Platform.Health.InvalidTenant", "Tenant inválido para health.");
        public static readonly Error InvalidCounter = new("Platform.Health.InvalidCounter", "Contador de requests inválido.");
    }

    /// <summary>Coupon catalog errors (9.5).</summary>
    public static class Coupon
    {
        public static readonly Error InvalidCode = new("Platform.Coupon.InvalidCode", "Código de cupom inválido.");
        public static readonly Error InvalidValue = new("Platform.Coupon.InvalidValue", "Valor de desconto inválido.");
        public static readonly Error InvalidMaxRedemptions = new("Platform.Coupon.InvalidMaxRedemptions", "Limite de usos inválido.");
        public static readonly Error InvalidReference = new("Platform.Coupon.InvalidReference", "Referência de cupom inválida.");
        public static readonly Error NotFound = new("Platform.Coupon.NotFound", "Cupom não encontrado.");
        public static readonly Error NotRedeemable = new("Platform.Coupon.NotRedeemable", "Cupom não pode ser resgatado.");
        public static readonly Error AlreadyRedeemed = new("Platform.Coupon.AlreadyRedeemed", "Cupom já resgatado para este tenant.");
        public static readonly Error Exhausted = new("Platform.Coupon.Exhausted", "Cupom esgotado.");
    }

    /// <summary>SaaS metrics and acquisition spend errors (10.2).</summary>
    public static class Metrics
    {
        public static readonly Error InvalidYear = new("Platform.Metrics.InvalidYear", "Ano inválido para métricas.");
        public static readonly Error InvalidMonth = new("Platform.Metrics.InvalidMonth", "Mês inválido para métricas.");
        public static readonly Error InvalidChannel = new("Platform.Metrics.InvalidChannel", "Canal de aquisição inválido.");
        public static readonly Error InvalidAmount = new("Platform.Metrics.InvalidAmount", "Valor de aquisição inválido.");
    }
}
