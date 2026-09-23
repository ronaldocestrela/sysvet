namespace Platform.Application.Abstractions;

/// <summary>Resolves coarse geo location from client IP (9.7).</summary>
public interface IGeoIpLookup
{
    /// <summary>Returns country and region codes for the IP.</summary>
    Task<(string Country, string Region)> LookupAsync(string clientIp, CancellationToken cancellationToken = default);
}
