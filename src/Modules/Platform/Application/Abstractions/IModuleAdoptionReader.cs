using Platform.Domain.Services;

namespace Platform.Application.Abstractions;

/// <summary>Loads module adoption matrix for Super Admin (10.3).</summary>
public interface IModuleAdoptionReader
{
    /// <summary>Builds adoption snapshot from effective entitlements.</summary>
    Task<ModuleAdoptionSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
