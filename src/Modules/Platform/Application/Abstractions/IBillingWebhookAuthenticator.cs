namespace Platform.Application.Abstractions;

/// <summary>Validates Asaas webhook access token (9.4).</summary>
public interface IBillingWebhookAuthenticator
{
    /// <summary>Returns true when token matches configured secret.</summary>
    bool IsAuthorized(string? accessToken);
}
