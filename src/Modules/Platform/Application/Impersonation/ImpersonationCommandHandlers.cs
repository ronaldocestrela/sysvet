using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Application.Configuration;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Impersonation;

/// <summary>Impersonation CQRS handlers (9.6).</summary>
public sealed class ImpersonationCommandHandlers :
    IRequestHandler<StartImpersonationCommand, Result<StartImpersonationResultDto>>,
    IRequestHandler<EndImpersonationCommand, Result>,
    IRequestHandler<ListImpersonationAuditsQuery, Result<IReadOnlyList<ImpersonationAuditDto>>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IImpersonationSessionRepository _sessionRepository;
    private readonly IImpersonationAuditRepository _auditRepository;
    private readonly IImpersonationAccessTokenIssuer _tokenIssuer;
    private readonly ICurrentUser _currentUser;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly ImpersonationOptions _options;

    /// <summary>Creates handlers.</summary>
    public ImpersonationCommandHandlers(
        ITenantRepository tenantRepository,
        IImpersonationSessionRepository sessionRepository,
        IImpersonationAuditRepository auditRepository,
        IImpersonationAccessTokenIssuer tokenIssuer,
        ICurrentUser currentUser,
        IPlatformUnitOfWork unitOfWork,
        IOptions<ImpersonationOptions> options)
    {
        _tenantRepository = tenantRepository;
        _sessionRepository = sessionRepository;
        _auditRepository = auditRepository;
        _tokenIssuer = tokenIssuer;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result<StartImpersonationResultDto>> Handle(StartImpersonationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.Roles.Contains(ApplicationRoles.SuperAdmin))
        {
            return Result.Failure<StartImpersonationResultDto>(PlatformErrorCodes.Impersonation.Forbidden);
        }

        var actorUserId = _currentUser.UserId;
        var actorEmail = _currentUser.Email;
        if (string.IsNullOrWhiteSpace(actorUserId) || string.IsNullOrWhiteSpace(actorEmail))
        {
            return Result.Failure<StartImpersonationResultDto>(PlatformErrorCodes.Impersonation.InvalidActor);
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TargetTenantId, cancellationToken);
        if (tenant is null || tenant.DeletedAt is not null || tenant.Status == TenantStatus.Cancelled)
        {
            return Result.Failure<StartImpersonationResultDto>(PlatformErrorCodes.Impersonation.TenantNotAllowed);
        }

        var sessionId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.SessionMinutes);
        var session = ImpersonationSession.Start(
            sessionId,
            actorUserId,
            actorEmail,
            request.TargetTenantId,
            request.ClientIp,
            now,
            expires);
        if (session.IsFailure)
        {
            return Result.Failure<StartImpersonationResultDto>(session.Error);
        }

        var audit = ImpersonationAuditEntry.CreateStarted(sessionId, actorUserId, request.TargetTenantId, request.ClientIp, now);
        if (audit.IsFailure)
        {
            return Result.Failure<StartImpersonationResultDto>(audit.Error);
        }

        await _sessionRepository.AddAsync(session.Value, cancellationToken);
        await _auditRepository.AddAsync(audit.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _tokenIssuer.Issue(
            actorUserId,
            actorEmail,
            request.TargetTenantId,
            sessionId,
            _options.SessionMinutes);

        return Result.Success(new StartImpersonationResultDto(
            token.AccessToken,
            token.ExpiresInSeconds,
            token.SessionId,
            request.TargetTenantId));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(EndImpersonationCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(PlatformErrorCodes.Impersonation.SessionNotFound);
        }

        var actorUserId = _currentUser.UserId ?? string.Empty;
        var isSuperAdmin = _currentUser.Roles.Contains(ApplicationRoles.SuperAdmin);
        var isSessionActor = string.Equals(actorUserId, session.ActorUserId, StringComparison.Ordinal);
        if (!isSuperAdmin && !isSessionActor)
        {
            return Result.Failure(PlatformErrorCodes.Impersonation.Forbidden);
        }

        var now = DateTimeOffset.UtcNow;
        var ended = session.End(now);
        if (ended.IsFailure)
        {
            return ended;
        }

        var audit = ImpersonationAuditEntry.CreateEnded(
            session.Id,
            session.ActorUserId,
            session.TargetTenantId,
            request.ClientIp,
            now);
        if (audit.IsFailure)
        {
            return audit;
        }

        await _auditRepository.AddAsync(audit.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ImpersonationAuditDto>>> Handle(
        ListImpersonationAuditsQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 500);
        var rows = await _auditRepository.ListAsync(take, cancellationToken);
        var dtos = rows.Select(r => new ImpersonationAuditDto(
            r.Id,
            r.SessionId,
            r.ActorUserId,
            r.TargetTenantId,
            r.Action,
            r.OccurredAt,
            r.ClientIp)).ToList();

        return Result.Success<IReadOnlyList<ImpersonationAuditDto>>(dtos);
    }
}
