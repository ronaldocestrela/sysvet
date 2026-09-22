using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Platform.Application.Provisioning;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Provisioning;

/// <summary>Seeds tenant profiles and optional SQL schema (ADR-047).</summary>
public sealed class TenantProvisioner : ITenantProvisioner
{
    private readonly PlatformDbContext _platformDbContext;
    private readonly IAccessProfileSeeder _accessProfileSeeder;
    private readonly ITenantContext _tenantContext;
    private readonly DatabaseOptions _databaseOptions;

    /// <summary>Creates the provisioner.</summary>
    public TenantProvisioner(
        PlatformDbContext platformDbContext,
        IAccessProfileSeeder accessProfileSeeder,
        ITenantContext tenantContext,
        IOptions<DatabaseOptions> databaseOptions)
    {
        _platformDbContext = platformDbContext;
        _accessProfileSeeder = accessProfileSeeder;
        _tenantContext = tenantContext;
        _databaseOptions = databaseOptions.Value;
    }

    /// <inheritdoc />
    public async Task<Result> ProvisionAsync(Guid tenantId, string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.Equals(_databaseOptions.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var createSchema = await EnsureSqlServerSchemaAsync(schemaName, cancellationToken);
            if (createSchema.IsFailure)
            {
                return createSchema;
            }
        }

        _tenantContext.TenantId = tenantId;
        _tenantContext.SchemaName = schemaName;
        await _accessProfileSeeder.EnsureTenantProfilesAsync(tenantId, cancellationToken);
        return Result.Success();
    }

    private async Task<Result> EnsureSqlServerSchemaAsync(string schemaName, CancellationToken cancellationToken)
    {
        if (!schemaName.StartsWith("tenant_", StringComparison.Ordinal))
        {
            return Result.Failure(Platform.Domain.ErrorCodes.Tenant.ProvisioningFailed);
        }

        var bracketed = schemaName.Replace("]", "]]");
#pragma warning disable EF1002 // schemaName is validated as tenant_{guid} before reaching SQL Server.
        await _platformDbContext.Database.ExecuteSqlRawAsync(
            $"IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'{schemaName}') EXEC(N'CREATE SCHEMA [{bracketed}]');",
            cancellationToken);
#pragma warning restore EF1002

        return Result.Success();
    }
}
