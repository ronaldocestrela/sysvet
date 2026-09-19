using Core.Domain;
using SharedUI.Services;

namespace MauiApp.Services;

/// <summary>
/// Mobile barcode capture via native prompt until dedicated camera UI is configured per platform.
/// </summary>
public sealed class MauiBarcodeScannerService : IBarcodeScannerService
{
    public bool IsAvailable =>
        DeviceInfo.Platform == DevicePlatform.Android
        || DeviceInfo.Platform == DevicePlatform.iOS
        || DeviceInfo.Platform == DevicePlatform.WinUI;

    public async Task<Result<string>> ScanAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return Result.Failure<string>(new Error("BarcodeScanner.Unavailable", "Scanner não disponível neste dispositivo."));
        }

        var value = await MainThread.InvokeOnMainThreadAsync(async () =>
            await Application.Current!.MainPage!.DisplayPromptAsync(
                "Código de barras",
                "Aponte a câmera ou digite o código lido:",
                "OK",
                "Cancelar",
                keyboard: Keyboard.Default));

        return string.IsNullOrWhiteSpace(value)
            ? Result.Failure<string>(new Error("BarcodeScanner.Cancelled", "Leitura cancelada."))
            : Result.Success(value.Trim());
    }
}
