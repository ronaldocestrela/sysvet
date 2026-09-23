namespace Platform.Application.Abstractions;

/// <summary>Resolves tenant id from login email without validating password (9.7).</summary>
public interface IPlatformLoginUserResolver
{
    /// <summary>Returns tenant id when a clinic user exists for the email.</summary>
    Task<Guid?> ResolveTenantIdAsync(string email, CancellationToken cancellationToken = default);
}
