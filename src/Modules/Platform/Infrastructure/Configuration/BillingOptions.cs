namespace Platform.Infrastructure.Configuration;

/// <summary>Platform SaaS billing gateway settings (ADR-049).</summary>
public sealed class BillingOptions
{
    /// <summary>Configuration section: Platform:Billing.</summary>
    public const string SectionName = "Platform:Billing";

    /// <summary>Fake (CI/dev) or Asaas.</summary>
    public string Provider { get; set; } = "Fake";

    /// <summary>Asaas API base URL.</summary>
    public string BaseUrl { get; set; } = "https://api.asaas.com";

    /// <summary>Asaas API key (user-secrets/env).</summary>
    public string? ApiKey { get; set; }

    /// <summary>Expected value for header asaas-access-token on webhooks.</summary>
    public string? WebhookAccessToken { get; set; }
}
