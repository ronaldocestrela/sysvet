using Core.Domain;

namespace SharedUI.Services;

/// <summary>
/// Host-specific barcode capture (camera on MAUI; manual entry on web).
/// </summary>
public interface IBarcodeScannerService
{
    /// <summary>Whether the host can offer a scan action (camera or dedicated UI).</summary>
    bool IsAvailable { get; }

    /// <summary>Opens scanner UI and returns the decoded barcode value.</summary>
    Task<Result<string>> ScanAsync(CancellationToken cancellationToken = default);
}
