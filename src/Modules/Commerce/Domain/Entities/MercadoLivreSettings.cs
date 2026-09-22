using Core.Domain;

namespace Commerce.Domain.Entities;

/// <summary>
/// Tenant singleton Mercado Livre API credentials (manual token PoC).
/// </summary>
public sealed class MercadoLivreSettings : AggregateRoot
{
    public const string SingletonKey = "default";

    public string Key { get; private set; } = SingletonKey;
    public string? AccessToken { get; private set; }
    public long? UserId { get; private set; }
    public string SiteId { get; private set; } = "MLB";
    public bool IsEnabled { get; private set; }

#pragma warning disable CS8618
    private MercadoLivreSettings() : base(Guid.Empty) { }
#pragma warning restore CS8618

    /// <summary>Creates empty settings row.</summary>
    public static MercadoLivreSettings CreateDefault(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Key = SingletonKey
        };

    /// <summary>Updates integration credentials.</summary>
    public void Update(string? accessToken, long? userId, string siteId, bool enabled)
    {
        AccessToken = string.IsNullOrWhiteSpace(accessToken) ? null : accessToken.Trim();
        UserId = userId;
        SiteId = string.IsNullOrWhiteSpace(siteId) ? "MLB" : siteId.Trim();
        IsEnabled = enabled && !string.IsNullOrWhiteSpace(AccessToken) && UserId.HasValue;
    }
}
