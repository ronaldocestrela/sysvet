using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Application.Auditing;

/// <summary>Platform login and change audit handlers (9.7).</summary>
public sealed class PlatformAuditingCommandHandlers :
    IRequestHandler<RecordPlatformLoginCommand, Result>,
    IRequestHandler<ListPlatformLoginLogsQuery, Result<PagedResult<PlatformLoginLogDto>>>,
    IRequestHandler<ListPlatformChangeAuditsQuery, Result<PagedResult<PlatformChangeAuditDto>>>
{
    private readonly IPlatformLoginLogRepository _loginLogRepository;
    private readonly IPlatformChangeAuditRepository _changeAuditRepository;
    private readonly IPlatformLoginUserResolver _loginUserResolver;
    private readonly IGeoIpLookup _geoIpLookup;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates handlers.</summary>
    public PlatformAuditingCommandHandlers(
        IPlatformLoginLogRepository loginLogRepository,
        IPlatformChangeAuditRepository changeAuditRepository,
        IPlatformLoginUserResolver loginUserResolver,
        IGeoIpLookup geoIpLookup,
        IPlatformUnitOfWork unitOfWork)
    {
        _loginLogRepository = loginLogRepository;
        _changeAuditRepository = changeAuditRepository;
        _loginUserResolver = loginUserResolver;
        _geoIpLookup = geoIpLookup;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(RecordPlatformLoginCommand request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId;
        if (tenantId is null || tenantId == Guid.Empty)
        {
            tenantId = await _loginUserResolver.ResolveTenantIdAsync(request.Email, cancellationToken);
        }

        var geo = await _geoIpLookup.LookupAsync(request.ClientIp, cancellationToken);
        var log = PlatformLoginLog.Create(
            tenantId,
            request.Email,
            request.Succeeded,
            request.ClientIp,
            request.UserAgent,
            geo.Country,
            geo.Region,
            DateTimeOffset.UtcNow);
        if (log.IsFailure)
        {
            return Result.Failure(log.Error);
        }

        await _loginLogRepository.AddAsync(log.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<PlatformLoginLogDto>>> Handle(
        ListPlatformLoginLogsQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.TryCreate(request.Page, request.PageSize);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<PlatformLoginLogDto>>(pageRequest.Error);
        }

        var (page, pageSize) = (pageRequest.Value.Page, pageRequest.Value.PageSize);
        var (rows, total) = await _loginLogRepository.ListPagedAsync(request.TenantId, page, pageSize, cancellationToken);
        var dtos = rows.Select(r => new PlatformLoginLogDto(
            r.Id,
            r.TenantId,
            r.Email,
            r.Succeeded,
            r.ClientIp,
            r.UserAgent,
            r.Country,
            r.Region,
            r.OccurredAt)).ToList();
        return Result.Success(new PagedResult<PlatformLoginLogDto>(dtos, page, pageSize, total));
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<PlatformChangeAuditDto>>> Handle(
        ListPlatformChangeAuditsQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.TryCreate(request.Page, request.PageSize);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<PlatformChangeAuditDto>>(pageRequest.Error);
        }

        var (page, pageSize) = (pageRequest.Value.Page, pageRequest.Value.PageSize);
        var (rows, total) = await _changeAuditRepository.ListPagedAsync(request.TenantId, page, pageSize, cancellationToken);
        var dtos = rows.Select(r => new PlatformChangeAuditDto(
            r.Id,
            r.ActorUserId,
            r.TenantId,
            r.Action,
            r.PayloadSummary,
            r.ClientIp,
            r.OccurredAt)).ToList();
        return Result.Success(new PagedResult<PlatformChangeAuditDto>(dtos, page, pageSize, total));
    }
}
