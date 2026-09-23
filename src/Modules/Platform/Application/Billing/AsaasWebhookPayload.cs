using System.Text.Json.Serialization;

namespace Platform.Application.Billing;

/// <summary>Minimal Asaas webhook envelope for SaaS billing (9.4).</summary>
public sealed class AsaasWebhookPayload
{
    /// <summary>Event name (e.g. PAYMENT_CONFIRMED).</summary>
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    /// <summary>Payment object.</summary>
    [JsonPropertyName("payment")]
    public AsaasWebhookPayment? Payment { get; set; }
}

/// <summary>Payment section of webhook.</summary>
public sealed class AsaasWebhookPayment
{
    /// <summary>Asaas payment id.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>External reference (invoice id).</summary>
    [JsonPropertyName("externalReference")]
    public string? ExternalReference { get; set; }
}
