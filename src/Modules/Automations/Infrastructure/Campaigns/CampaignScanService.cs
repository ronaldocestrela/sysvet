using Automations.Application.Abstractions;
using Automations.Application.Campaigns;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Automations.Infrastructure.Configuration;
using Automations.Infrastructure.Reminders;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Campaigns;

/// <summary>
/// Scans completed services and enqueues NPS invitations for active post-appointment campaigns.
/// </summary>
public sealed class CampaignScanService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly INpsInviteRepository _inviteRepository;
    private readonly ITutorVisitReadPort _visitReadPort;
    private readonly IPetRepository _petRepository;
    private readonly INpsSurveyTokenService _tokenService;
    private readonly CampaignDispatcher _dispatcher;
    private readonly IAutomationsSettingsRepository _settingsRepository;
    private readonly IAutomationsUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly AutomationsOptions _options;

    public CampaignScanService(
        ICampaignRepository campaignRepository,
        INpsInviteRepository inviteRepository,
        ITutorVisitReadPort visitReadPort,
        IPetRepository petRepository,
        INpsSurveyTokenService tokenService,
        CampaignDispatcher dispatcher,
        IAutomationsSettingsRepository settingsRepository,
        IAutomationsUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IOptions<AutomationsOptions> options)
    {
        _campaignRepository = campaignRepository;
        _inviteRepository = inviteRepository;
        _visitReadPort = visitReadPort;
        _petRepository = petRepository;
        _tokenService = tokenService;
        _dispatcher = dispatcher;
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _options = options.Value;
    }

    /// <summary>
    /// Creates NPS invites and outbound jobs for today's completed services.
    /// </summary>
    public async Task<int> ScanAsync(CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId == Guid.Empty)
        {
            return 0;
        }

        var campaign = await _campaignRepository.GetActiveBySegmentAsync(CampaignSegmentKind.PostAppointment, cancellationToken);
        if (campaign is null || !campaign.IsActivePostAppointmentNps())
        {
            return 0;
        }

        var tz = await ResolveTimeZoneAsync(cancellationToken);
        var localToday = ReminderTimeHelper.ToLocalDate(DateTimeOffset.UtcNow, tz);
        var fromUtc = ReminderTimeHelper.DayStartUtc(localToday, tz);
        var toUtc = ReminderTimeHelper.DayStartUtc(localToday.AddDays(1), tz);
        var services = await _visitReadPort.ListCompletedBetweenAsync(fromUtc, toUtc, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddDays(_options.NpsInviteExpiryDays);
        var targets = new List<CampaignDispatchTarget>();
        var createdInvites = false;

        foreach (var service in services)
        {
            if (await _inviteRepository.ExistsForSourceAsync(service.SourceType, service.SourceId, cancellationToken))
            {
                continue;
            }

            var pet = await _petRepository.GetByIdAsync(service.PetId, cancellationToken);
            var petName = pet?.Name ?? string.Empty;

            var inviteId = Guid.NewGuid();
            var token = _tokenService.CreateToken(_tenantContext.TenantId, inviteId, expiresAt);
            var tokenHash = _tokenService.ComputeTokenHash(token);
            var invite = NpsInvite.Create(
                service.TutorId,
                tokenHash,
                expiresAt,
                service.SourceType,
                service.SourceId,
                campaign.Id,
                inviteId,
                now);
            if (invite.IsFailure)
            {
                continue;
            }

            _inviteRepository.Add(invite.Value);
            createdInvites = true;

            var surveyUrl = BuildSurveyUrl(token);
            var idempotencyBase = service.SourceType switch
            {
                "Appointment" => $"nps:appointment:{service.SourceId:N}",
                "GroomingAppointment" => $"nps:grooming:{service.SourceId:N}",
                _ => $"nps:{service.SourceId:N}"
            };

            targets.Add(new CampaignDispatchTarget(
                service.TutorId,
                campaign.TemplateCode,
                idempotencyBase,
                new Dictionary<string, string>
                {
                    ["PetName"] = petName,
                    ["SurveyUrl"] = surveyUrl
                },
                "NpsInvite",
                invite.Value.Id));
        }

        var enqueued = await _dispatcher.EnqueueAsync(targets, cancellationToken);
        if (createdInvites)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return enqueued;
    }

    private string BuildSurveyUrl(string token)
    {
        var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/nps/{token}";
    }

    private async Task<TimeZoneInfo> ResolveTimeZoneAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetSingletonAsync(cancellationToken);
        if (settings is not null)
        {
            return settings.ResolveTimeZone();
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}
