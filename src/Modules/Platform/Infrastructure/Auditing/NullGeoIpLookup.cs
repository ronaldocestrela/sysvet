using Platform.Application.Abstractions;

namespace Platform.Infrastructure.Auditing;

/// <summary>Placeholder geo lookup until an external provider is integrated (9.7).</summary>
public sealed class NullGeoIpLookup : IGeoIpLookup
{
    /// <inheritdoc />
    public Task<(string Country, string Region)> LookupAsync(string clientIp, CancellationToken cancellationToken = default) =>
        Task.FromResult(("unknown", "unknown"));
}
