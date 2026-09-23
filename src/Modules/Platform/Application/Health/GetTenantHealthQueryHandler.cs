using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Repositories;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Health;

/// <summary>Aggregates tenant health metrics from contributors and counters (9.7).</summary>
public sealed class GetTenantHealthQueryHandler : IRequestHandler<GetTenantHealthQuery, Result<TenantHealthDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantRequestDailyRepository _requestDailyRepository;
    private readonly IEnumerable<ITenantDataVolumeContributor> _volumeContributors;

    /// <summary>Creates the handler.</summary>
    public GetTenantHealthQueryHandler(
        ITenantRepository tenantRepository,
        ITenantRequestDailyRepository requestDailyRepository,
        IEnumerable<ITenantDataVolumeContributor> volumeContributors)
    {
        _tenantRepository = tenantRepository;
        _requestDailyRepository = requestDailyRepository;
        _volumeContributors = volumeContributors;
    }

    /// <inheritdoc />
    public async Task<Result<TenantHealthDto>> Handle(GetTenantHealthQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result.Failure<TenantHealthDto>(PlatformErrorCodes.Health.InvalidTenant);
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure<TenantHealthDto>(PlatformErrorCodes.Tenant.NotFound);
        }

        var slices = new List<TenantDataVolumeSliceDto>();
        long totalRows = 0;
        foreach (var contributor in _volumeContributors)
        {
            var count = await contributor.CountRowsAsync(request.TenantId, cancellationToken);
            totalRows += count;
            slices.Add(new TenantDataVolumeSliceDto(contributor.ModuleName, count));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayRow = await _requestDailyRepository.GetForDateAsync(request.TenantId, today, cancellationToken);
        var requestsToday = todayRow?.RequestCount ?? 0;
        var requestsLast7 = await _requestDailyRepository.SumLastDaysAsync(request.TenantId, 7, cancellationToken);

        var dto = new TenantHealthDto(
            request.TenantId,
            slices,
            totalRows,
            totalRows * TenantHealthMetrics.BytesPerRowEstimate,
            requestsToday,
            requestsLast7,
            DateTimeOffset.UtcNow);

        return Result.Success(dto);
    }
}
