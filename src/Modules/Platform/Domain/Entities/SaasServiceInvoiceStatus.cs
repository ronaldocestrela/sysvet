namespace Platform.Domain.Entities;

/// <summary>Lifecycle of a VetNexus NFS-e tied to a platform billing invoice (9.6).</summary>
public enum SaasServiceInvoiceStatus
{
    /// <summary>Queued or awaiting gateway response.</summary>
    Pending = 0,

    /// <summary>Authorized by NFS-e Nacional (or Fake).</summary>
    Authorized = 1,

    /// <summary>Emission failed; billing payment remains settled.</summary>
    Failed = 2
}
