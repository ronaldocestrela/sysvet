using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Repositories;
using Platform.Domain.Services;

namespace Platform.Application.Metrics;

/// <summary>Handles SaaS metrics query (10.2).</summary>
public sealed class GetPlatformSaasMetricsQueryHandler : IRequestHandler<GetPlatformSaasMetricsQuery, Result<PlatformSaasMetricsDto>>
{
    private readonly ISaasMetricsReader _reader;

    /// <summary>Creates the handler.</summary>
    public GetPlatformSaasMetricsQueryHandler(ISaasMetricsReader reader) => _reader = reader;

    /// <inheritdoc />
    public async Task<Result<PlatformSaasMetricsDto>> Handle(GetPlatformSaasMetricsQuery request, CancellationToken cancellationToken)
    {
        if (request.Year is < 2000 or > 9999)
        {
            return Result.Failure<PlatformSaasMetricsDto>(Platform.Domain.ErrorCodes.Metrics.InvalidYear);
        }

        if (request.Month is < 1 or > 12)
        {
            return Result.Failure<PlatformSaasMetricsDto>(Platform.Domain.ErrorCodes.Metrics.InvalidMonth);
        }

        var snapshot = await _reader.GetSnapshotAsync(request.Year, request.Month, cancellationToken);
        return Result.Success(Map(snapshot));
    }

    private static PlatformSaasMetricsDto Map(SaasMetricsSnapshot snapshot) =>
        new(
            snapshot.Year,
            snapshot.Month,
            snapshot.BilledMrr,
            snapshot.ContractedMrr,
            snapshot.Arr,
            snapshot.CashIn,
            snapshot.CashOut,
            snapshot.NetCashFlow,
            snapshot.LogoChurnRate,
            snapshot.PayingTenantsInMonth,
            snapshot.CancelledLogosInMonth,
            snapshot.PayingLogosAtMonthStart,
            snapshot.Ltv,
            snapshot.AcquisitionSpend,
            snapshot.NewPayingTenantsInMonth,
            snapshot.Cac,
            snapshot.Delinquency.Select(d => new SaasDelinquencyItemDto(
                d.TenantId,
                d.DisplayName,
                d.OutstandingAmount,
                d.PastDueSince,
                d.BillingStanding)).ToList());
}

/// <summary>Handles acquisition spend upsert (10.2).</summary>
public sealed class UpsertAcquisitionSpendCommandHandler : IRequestHandler<UpsertAcquisitionSpendCommand, Result>
{
    private readonly IAcquisitionSpendRepository _repository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public UpsertAcquisitionSpendCommandHandler(
        IAcquisitionSpendRepository repository,
        IPlatformUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UpsertAcquisitionSpendCommand request, CancellationToken cancellationToken)
    {
        var created = Platform.Domain.Entities.AcquisitionSpend.Create(
            request.Year,
            request.Month,
            request.Channel,
            request.Amount,
            request.Note);

        if (created.IsFailure)
        {
            return created;
        }

        var normalizedChannel = created.Value.Channel;
        var existing = await _repository.GetByMonthAndChannelAsync(
            request.Year,
            request.Month,
            normalizedChannel,
            cancellationToken);

        if (existing is null)
        {
            await _repository.AddAsync(created.Value, cancellationToken);
        }
        else
        {
            var updated = existing.Update(request.Amount, request.Note);
            if (updated.IsFailure)
            {
                return updated;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
