using Core.Domain;
using SharedUI.Services;

namespace BlazorWeb.Services;

/// <summary>
/// Web host has no camera scanner; users type barcodes manually.
/// </summary>
public sealed class WebBarcodeScannerService : IBarcodeScannerService
{
    public bool IsAvailable => false;

    public Task<Result<string>> ScanAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Failure<string>(new Error("BarcodeScanner.Unavailable", "Leitura por câmera disponível apenas no app mobile.")));
}
