using Automations.Application.Abstractions;
using Automations.Domain.Repositories;
using ErrorCodes = Automations.Domain.ErrorCodes;
using Core.Application.Messaging;
using Core.Domain;
using MediatR;

namespace Automations.Application.Nps.Commands;

/// <summary>
/// Records a tutor NPS response from a public tokenized link (no staff JWT).
/// </summary>
public sealed record SubmitNpsResponseCommand(string Token, int Score, string? Comment) : ICommand;

public sealed class SubmitNpsResponseCommandHandler : IRequestHandler<SubmitNpsResponseCommand, Result>
{
    private readonly INpsSurveyTokenService _tokenService;
    private readonly INpsInviteRepository _inviteRepository;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public SubmitNpsResponseCommandHandler(
        INpsSurveyTokenService tokenService,
        INpsInviteRepository inviteRepository,
        IAutomationsUnitOfWork unitOfWork)
    {
        _tokenService = tokenService;
        _inviteRepository = inviteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitNpsResponseCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var parsed = _tokenService.Validate(request.Token, now);
        if (parsed.IsFailure)
        {
            return Result.Failure(parsed.Error);
        }

        var invite = await _inviteRepository.GetByIdAsync(parsed.Value.InviteId, cancellationToken);
        if (invite is null)
        {
            return Result.Failure(ErrorCodes.Nps.NotFound);
        }

        var respond = invite.Respond(request.Score, request.Comment, now);
        if (respond.IsFailure)
        {
            return respond;
        }

        _inviteRepository.Update(invite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
