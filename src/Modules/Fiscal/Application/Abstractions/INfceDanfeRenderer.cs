using Core.Domain;

namespace Fiscal.Application.Abstractions;

/// <summary>Renders NFC-e consumer receipt PDF from authorized or contingency XML.</summary>
public interface INfceDanfeRenderer
{
    /// <summary>Produces PDF bytes including QR code URL when available.</summary>
    Task<Result<byte[]>> RenderAsync(string xml, string? qrCodeUrl, CancellationToken cancellationToken = default);
}
