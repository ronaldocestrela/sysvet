using Automations.Application.Abstractions;
using Automations.Application.Nps.Dtos;
using Automations.Domain.Repositories;
using ErrorCodes = Automations.Domain.ErrorCodes;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entities;
using MediatR;

namespace Automations.Application.Nps.Queries;

/// <summary>
/// NPS report for responded invites in a period.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record GetNpsReportQuery(DateTimeOffset From, DateTimeOffset To) : IQuery<NpsReportDto>;

/// <summary>
/// Return frequency per tutor in a period.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record GetReturnFrequencyReportQuery(DateTimeOffset From, DateTimeOffset To)
    : IQuery<IReadOnlyList<ReturnFrequencyReportRowDto>>;

/// <summary>
/// Public preview for a tokenized NPS link (no staff auth).
/// </summary>
public sealed record GetNpsPublicPreviewQuery(string Token) : IQuery<NpsPublicPreviewDto>;

public sealed class GetNpsReportQueryHandler : IRequestHandler<GetNpsReportQuery, Result<NpsReportDto>>
{
    private readonly INpsInviteRepository _repository;

    public GetNpsReportQueryHandler(INpsInviteRepository repository) => _repository = repository;

    public async Task<Result<NpsReportDto>> Handle(GetNpsReportQuery request, CancellationToken cancellationToken)
    {
        var invites = await _repository.ListRespondedBetweenAsync(request.From, request.To, cancellationToken);
        var scores = invites.Where(i => i.Score is not null).Select(i => i.Score!.Value).ToList();
        if (scores.Count == 0)
        {
            return Result.Success(new NpsReportDto());
        }

        var promoters = scores.Count(s => s >= 9);
        var detractors = scores.Count(s => s <= 6);
        var passives = scores.Count - promoters - detractors;
        var nps = (promoters / (double)scores.Count * 100) - (detractors / (double)scores.Count * 100);

        var comments = invites
            .Where(i => i.RespondedAt is not null)
            .Select(i => new NpsCommentDto
            {
                Score = i.Score ?? 0,
                Comment = i.Comment,
                RespondedAt = i.RespondedAt!.Value
            })
            .ToList();

        return Result.Success(new NpsReportDto
        {
            TotalResponses = scores.Count,
            Promoters = promoters,
            Passives = passives,
            Detractors = detractors,
            NpsScore = Math.Round(nps, 1),
            Comments = comments
        });
    }
}

public sealed class GetReturnFrequencyReportQueryHandler
    : IRequestHandler<GetReturnFrequencyReportQuery, Result<IReadOnlyList<ReturnFrequencyReportRowDto>>>
{
    private readonly ITutorVisitReadPort _visitReadPort;
    private readonly ITutorRepository _tutorRepository;

    public GetReturnFrequencyReportQueryHandler(ITutorVisitReadPort visitReadPort, ITutorRepository tutorRepository)
    {
        _visitReadPort = visitReadPort;
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<IReadOnlyList<ReturnFrequencyReportRowDto>>> Handle(
        GetReturnFrequencyReportQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _visitReadPort.GetReturnFrequencyAsync(request.From, request.To, cancellationToken);
        var result = new List<ReturnFrequencyReportRowDto>();
        foreach (var row in rows)
        {
            var tutor = await _tutorRepository.GetByIdAsync(row.TutorId, cancellationToken);
            result.Add(new ReturnFrequencyReportRowDto
            {
                TutorId = row.TutorId,
                TutorName = tutor?.Name ?? string.Empty,
                VisitCount = row.VisitCount,
                LastVisitAt = row.LastVisitAt,
                AverageDaysBetweenVisits = row.AverageDaysBetweenVisits
            });
        }

        return Result.Success<IReadOnlyList<ReturnFrequencyReportRowDto>>(result);
    }
}

public sealed class GetNpsPublicPreviewQueryHandler : IRequestHandler<GetNpsPublicPreviewQuery, Result<NpsPublicPreviewDto>>
{
    private readonly INpsSurveyTokenService _tokenService;
    private readonly INpsInviteRepository _inviteRepository;
    private readonly ITutorRepository _tutorRepository;

    public GetNpsPublicPreviewQueryHandler(
        INpsSurveyTokenService tokenService,
        INpsInviteRepository inviteRepository,
        ITutorRepository tutorRepository)
    {
        _tokenService = tokenService;
        _inviteRepository = inviteRepository;
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<NpsPublicPreviewDto>> Handle(GetNpsPublicPreviewQuery request, CancellationToken cancellationToken)
    {
        var parsed = _tokenService.Validate(request.Token, DateTimeOffset.UtcNow);
        if (parsed.IsFailure)
        {
            return Result.Failure<NpsPublicPreviewDto>(parsed.Error);
        }

        var invite = await _inviteRepository.GetByIdAsync(parsed.Value.InviteId, cancellationToken);
        if (invite is null)
        {
            return Result.Failure<NpsPublicPreviewDto>(ErrorCodes.Nps.NotFound);
        }

        invite.MarkExpired(DateTimeOffset.UtcNow);
        var tutor = await _tutorRepository.GetByIdAsync(invite.TutorId, cancellationToken);
        var canRespond = invite.Status == Domain.Enums.NpsInviteStatus.Pending;

        return Result.Success(new NpsPublicPreviewDto
        {
            TutorName = tutor?.Name ?? string.Empty,
            CanRespond = canRespond,
            Status = invite.Status.ToString()
        });
    }
}
