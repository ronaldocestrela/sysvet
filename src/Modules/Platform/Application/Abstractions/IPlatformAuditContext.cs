namespace Platform.Application.Abstractions;

/// <summary>Super Admin operator context for platform audit rows (9.7).</summary>
public interface IPlatformAuditContext
{
    /// <summary>Authenticated operator id.</summary>
    string ActorUserId { get; }

    /// <summary>Client IP for the current HTTP request.</summary>
    string ClientIp { get; }
}
