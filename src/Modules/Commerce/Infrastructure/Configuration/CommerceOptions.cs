namespace Commerce.Infrastructure.Configuration;

/// <summary>Commerce module configuration section.</summary>
public sealed class CommerceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Commerce";

    /// <summary>Outbox worker poll interval seconds.</summary>
    public int PollIntervalSeconds { get; set; } = 5;

    /// <summary>Batch size for marketplace sync jobs.</summary>
    public int SyncBatchSize { get; set; } = 10;

    /// <summary>Mercado Livre API base URL.</summary>
    public string MercadoLivreApiBaseUrl { get; set; } = "https://api.mercadolibre.com";

    /// <summary>When true, registers fake marketplace channel instead of HTTP adapter.</summary>
    public bool UseFakeMarketplaceChannel { get; set; }
}
