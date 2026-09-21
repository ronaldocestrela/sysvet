using Core.Domain;
using Fiscal.Application.Abstractions;
using System.Text;

namespace Fiscal.Infrastructure.Reports;

/// <summary>Minimal PDF placeholder for fake NF-e DANFE.</summary>
public sealed class FakeDanfeRenderer : IDanfeRenderer
{
    public Task<Result<byte[]>> RenderAsync(string authorizedXml, CancellationToken cancellationToken = default)
    {
        var pdfHeader = "%PDF-1.4\n% Fake DANFE for development\n";
        return Task.FromResult(Result.Success(Encoding.UTF8.GetBytes(pdfHeader)));
    }
}
