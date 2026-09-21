using Core.Domain;
using Fiscal.Application.Abstractions;
using System.Text;

namespace Fiscal.Infrastructure.Reports;

/// <summary>Minimal NFC-e receipt PDF placeholder for CI.</summary>
public sealed class FakeNfceDanfeRenderer : INfceDanfeRenderer
{
    public Task<Result<byte[]>> RenderAsync(string xml, string? qrCodeUrl, CancellationToken cancellationToken = default)
    {
        var content = $"NFC-e receipt\nQR:{qrCodeUrl ?? "n/a"}\n{xml[..Math.Min(xml.Length, 200)]}";
        return Task.FromResult(Result.Success(Encoding.UTF8.GetBytes(content)));
    }
}
