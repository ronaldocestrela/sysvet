using Automations.Application.Campaigns.Dtos;
using Automations.Domain.Enums;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Authorization;
using Automations.Domain.Repositories;
using MediatR;

namespace Automations.Application.Campaigns.Queries;

/// <summary>
/// Lists tenant campaigns.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record ListCampaignsQuery : IQuery<IReadOnlyList<CampaignDto>>;

/// <summary>
/// Loads a campaign by id.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record GetCampaignByIdQuery(Guid CampaignId) : IQuery<CampaignDto>;

/// <summary>
/// Lists launch history for a campaign.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record ListCampaignRunsQuery(Guid CampaignId) : IQuery<IReadOnlyList<CampaignRunDto>>;

public sealed class ListCampaignsQueryHandler : IRequestHandler<ListCampaignsQuery, Result<IReadOnlyList<CampaignDto>>>
{
    private readonly ICampaignRepository _repository;

    public ListCampaignsQueryHandler(ICampaignRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<CampaignDto>>> Handle(ListCampaignsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<CampaignDto>>(items.Select(CampaignMappings.ToDto).ToList());
    }
}

public sealed class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, Result<CampaignDto>>
{
    private readonly ICampaignRepository _repository;

    public GetCampaignByIdQueryHandler(ICampaignRepository repository) => _repository = repository;

    public async Task<Result<CampaignDto>> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _repository.GetByIdAsync(request.CampaignId, cancellationToken);
        return campaign is null
            ? Result.Failure<CampaignDto>(Automations.Domain.ErrorCodes.Campaign.NotFound)
            : Result.Success(CampaignMappings.ToDto(campaign));
    }
}

public sealed class ListCampaignRunsQueryHandler : IRequestHandler<ListCampaignRunsQuery, Result<IReadOnlyList<CampaignRunDto>>>
{
    private readonly ICampaignRepository _repository;

    public ListCampaignRunsQueryHandler(ICampaignRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<CampaignRunDto>>> Handle(ListCampaignRunsQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _repository.GetByIdWithRunsAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result.Failure<IReadOnlyList<CampaignRunDto>>(Automations.Domain.ErrorCodes.Campaign.NotFound);
        }

        var runs = campaign.Runs
            .OrderByDescending(r => r.StartedAt)
            .Select(CampaignMappings.ToDto)
            .ToList();
        return Result.Success<IReadOnlyList<CampaignRunDto>>(runs);
    }
}

internal static class CampaignMappings
{
    public static CampaignDto ToDto(Automations.Domain.Entities.Campaign campaign) => new()
    {
        Id = campaign.Id,
        Name = campaign.Name,
        SegmentKind = campaign.SegmentKind,
        Status = campaign.Status,
        TemplateCode = campaign.TemplateCode,
        InactiveDays = campaign.InactiveDays,
        CooldownDays = campaign.CooldownDays
    };

    public static CampaignRunDto ToDto(Automations.Domain.Entities.CampaignRun run) => new()
    {
        Id = run.Id,
        CampaignId = run.CampaignId,
        StartedAt = run.StartedAt,
        AudienceCount = run.AudienceCount,
        EnqueuedCount = run.EnqueuedCount
    };
}
