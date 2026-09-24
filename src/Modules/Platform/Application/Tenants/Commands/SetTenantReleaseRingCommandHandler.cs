using System.Text.Json;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Application.Auditing;
using Platform.Domain.Repositories;

namespace Platform.Application.Tenants.Commands;

/// <summary>Updates tenant release ring and records Super Admin audit.</summary>
public sealed class SetTenantReleaseRingCommandHandler : IRequestHandler<SetTenantReleaseRingCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly PlatformBackofficeAuditRecorder _auditRecorder;

    /// <summary>Creates the handler.</summary>
    public SetTenantReleaseRingCommandHandler(
        ITenantRepository tenantRepository,
        IPlatformUnitOfWork unitOfWork,
        PlatformBackofficeAuditRecorder auditRecorder)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _auditRecorder = auditRecorder;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(SetTenantReleaseRingCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(Platform.Domain.ErrorCodes.Tenant.NotFound);
        }

        var change = tenant.SetReleaseRing(request.Ring);
        if (change.IsFailure)
        {
            return change;
        }

        await _auditRecorder.RecordAsync(
            request.TenantId,
            PlatformChangeActions.ReleaseRingSet,
            JsonSerializer.Serialize(new { ring = request.Ring.ToString() }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
