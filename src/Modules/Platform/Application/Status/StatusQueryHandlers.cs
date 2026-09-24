using Core.Domain;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Platform.Domain.Repositories;

namespace Platform.Application.Status;

/// <summary>Lists status incidents.</summary>
public sealed class ListStatusIncidentsQueryHandler : IRequestHandler<ListStatusIncidentsQuery, Result<IReadOnlyList<StatusIncidentDto>>>
{
    private readonly IStatusIncidentRepository _repository;

    /// <summary>Creates the handler.</summary>
    public ListStatusIncidentsQueryHandler(IStatusIncidentRepository repository) => _repository = repository;

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<StatusIncidentDto>>> Handle(ListStatusIncidentsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _repository.ListRecentAsync(request.Take, cancellationToken);
        return Result.Success<IReadOnlyList<StatusIncidentDto>>(
            rows.Select(CreateStatusIncidentCommandHandler.Map).ToList());
    }
}

/// <summary>Aggregates health checks and open incidents for the public status API.</summary>
public sealed class GetPublicStatusQueryHandler : IRequestHandler<GetPublicStatusQuery, Result<PublicStatusDto>>
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IStatusIncidentRepository _incidentRepository;

    /// <summary>Creates the handler.</summary>
    public GetPublicStatusQueryHandler(HealthCheckService healthCheckService, IStatusIncidentRepository incidentRepository)
    {
        _healthCheckService = healthCheckService;
        _incidentRepository = incidentRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PublicStatusDto>> Handle(GetPublicStatusQuery request, CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
        var components = BuildPublicComponents(report);
        var open = await _incidentRepository.ListOpenAsync(cancellationToken);
        var incidents = open.Select(i => new PublicStatusIncidentDto(
            i.Id,
            i.Title,
            i.Impact,
            i.Components,
            i.StartedAt)).ToList();

        var overall = ResolveOverallStatus(report.Status, incidents);
        return Result.Success(new PublicStatusDto(overall, components, incidents));
    }

    private static IReadOnlyList<PublicStatusComponentDto> BuildPublicComponents(HealthReport report)
    {
        var map = new (string PublicKey, string[] SourceNames)[]
        {
            ("api", ["api"]),
            ("database", ["core-db"]),
            ("cache", ["redis"]),
            ("sync", ["ops-sync-push"]),
            ("billing", ["ops-billing-failures"])
        };

        var list = new List<PublicStatusComponentDto>();
        foreach (var (publicKey, sources) in map)
        {
            var match = report.Entries.FirstOrDefault(e => sources.Contains(e.Key, StringComparer.OrdinalIgnoreCase));
            if (match.Key is null)
            {
                if (publicKey == "cache")
                {
                    continue;
                }

                list.Add(new PublicStatusComponentDto(publicKey, HealthStatus.Healthy.ToString()));
                continue;
            }

            list.Add(new PublicStatusComponentDto(publicKey, match.Value.Status.ToString()));
        }

        return list;
    }

    private static string ResolveOverallStatus(HealthStatus healthStatus, IReadOnlyList<PublicStatusIncidentDto> openIncidents)
    {
        if (openIncidents.Any(i => i.Impact is Platform.Domain.Entities.StatusIncidentImpact.Critical))
        {
            return HealthStatus.Unhealthy.ToString();
        }

        if (openIncidents.Any(i => i.Impact is Platform.Domain.Entities.StatusIncidentImpact.Major)
            || healthStatus == HealthStatus.Unhealthy)
        {
            return HealthStatus.Unhealthy.ToString();
        }

        if (openIncidents.Any(i => i.Impact is Platform.Domain.Entities.StatusIncidentImpact.Minor)
            || healthStatus == HealthStatus.Degraded)
        {
            return HealthStatus.Degraded.ToString();
        }

        return HealthStatus.Healthy.ToString();
    }
}
