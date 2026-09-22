using Automations.Application.Campaigns.Dtos;
using Automations.Domain.Entities;
using ErrorCodes = Automations.Domain.ErrorCodes;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Domain;
using MediatR;

namespace Automations.Application.Campaigns.Commands;

public sealed class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, Result<Guid>>
{
    private readonly ICampaignRepository _repository;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public CreateCampaignCommandHandler(ICampaignRepository repository, IAutomationsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var created = Campaign.Create(
            request.Name,
            request.SegmentKind,
            request.TemplateCode,
            request.InactiveDays,
            request.CooldownDays);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        if (request.SegmentKind == CampaignSegmentKind.PostAppointment)
        {
            created.Value.Update(
                created.Value.Name,
                created.Value.TemplateCode,
                created.Value.InactiveDays,
                created.Value.CooldownDays,
                CampaignStatus.Active);
        }

        _repository.Add(created.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}

public sealed class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand, Result>
{
    private readonly ICampaignRepository _repository;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public UpdateCampaignCommandHandler(ICampaignRepository repository, IAutomationsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _repository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result.Failure(ErrorCodes.Campaign.NotFound);
        }

        var update = campaign.Update(
            request.Name,
            request.TemplateCode,
            request.InactiveDays,
            request.CooldownDays,
            request.Status);
        if (update.IsFailure)
        {
            return update;
        }

        _repository.Update(campaign);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class LaunchCampaignCommandHandler : IRequestHandler<LaunchCampaignCommand, Result<CampaignRunDto>>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IMessageJobRepository _jobRepository;
    private readonly InactiveCampaignAudienceResolver _audienceResolver;
    private readonly CampaignDispatcher _dispatcher;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public LaunchCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        IMessageJobRepository jobRepository,
        InactiveCampaignAudienceResolver audienceResolver,
        CampaignDispatcher dispatcher,
        IAutomationsUnitOfWork unitOfWork)
    {
        _campaignRepository = campaignRepository;
        _jobRepository = jobRepository;
        _audienceResolver = audienceResolver;
        _dispatcher = dispatcher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CampaignRunDto>> Handle(LaunchCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result.Failure<CampaignRunDto>(ErrorCodes.Campaign.NotFound);
        }

        if (campaign.SegmentKind != CampaignSegmentKind.Inactive90Days)
        {
            return Result.Failure<CampaignRunDto>(ErrorCodes.Campaign.WrongSegment);
        }

        var now = DateTimeOffset.UtcNow;
        var audience = await _audienceResolver.ResolveAsync(campaign, now, cancellationToken);
        var cooldownSince = now.AddDays(-campaign.CooldownDays);
        var targets = new List<CampaignDispatchTarget>();

        foreach (var member in audience)
        {
            var prefix = $"campaign:{campaign.Id:N}:{member.TutorId:N}:";
            if (await _jobRepository.ExistsIdempotencyKeyPrefixSinceAsync(prefix, cooldownSince, cancellationToken))
            {
                continue;
            }

            targets.Add(new CampaignDispatchTarget(
                member.TutorId,
                campaign.TemplateCode,
                $"campaign:{campaign.Id:N}:{member.TutorId:N}",
                new Dictionary<string, string>
                {
                    ["LastVisitLocal"] = member.LastVisitAt.ToString("O")
                },
                "Campaign",
                campaign.Id));
        }

        var enqueued = await _dispatcher.EnqueueAsync(targets, cancellationToken);
        var run = CampaignRun.Create(campaign.Id, now, audience.Count, enqueued);
        _campaignRepository.AddRun(run);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CampaignRunDto
        {
            Id = run.Id,
            CampaignId = run.CampaignId,
            StartedAt = run.StartedAt,
            AudienceCount = run.AudienceCount,
            EnqueuedCount = run.EnqueuedCount
        });
    }
}
