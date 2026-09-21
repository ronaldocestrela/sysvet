using Core.Domain;

namespace Fiscal.Application.Abstractions;

/// <summary>Renders NF-e DANFE PDF from authorized XML.</summary>
public interface IDanfeRenderer
{
    /// <summary>Produces PDF bytes for storage.</summary>
    Task<Result<byte[]>> RenderAsync(string authorizedXml, CancellationToken cancellationToken = default);
}
