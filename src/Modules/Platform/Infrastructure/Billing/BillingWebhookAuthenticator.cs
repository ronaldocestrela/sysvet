using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Configuration;

namespace Platform.Infrastructure.Billing;

/// <summary>Compares webhook token using fixed-time equality (9.4).</summary>
public sealed class BillingWebhookAuthenticator : IBillingWebhookAuthenticator
{
    private readonly BillingOptions _options;

    /// <summary>Creates authenticator.</summary>
    public BillingWebhookAuthenticator(IOptions<BillingOptions> options) => _options = options.Value;

    /// <inheritdoc />
    public bool IsAuthorized(string? accessToken)
    {
        var expected = _options.WebhookAccessToken ?? string.Empty;
        if (string.IsNullOrEmpty(expected))
        {
            return string.IsNullOrEmpty(accessToken);
        }

        if (accessToken is null)
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(accessToken);
        return expectedBytes.Length == actualBytes.Length
               && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
