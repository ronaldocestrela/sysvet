namespace Platform.Domain.Entities;

/// <summary>Supported SaaS billing payment rails (9.4).</summary>
public enum BillingPaymentMethodKind
{
    /// <summary>Recurring charge via Asaas credit card token.</summary>
    CreditCard = 1,

    /// <summary>Pix QR / copy-paste per invoice cycle.</summary>
    Pix = 2,

    /// <summary>Registered boleto per invoice cycle.</summary>
    Boleto = 3
}
