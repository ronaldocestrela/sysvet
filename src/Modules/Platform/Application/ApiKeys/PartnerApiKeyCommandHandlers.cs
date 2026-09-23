using System.Text.Json;
using Core.Application.Common.Interfaces;
using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Application.Auditing;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.Security;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.ApiKeys;

/// <summary>Partner API key CQRS handlers (9.7).</summary>
public sealed class PartnerApiKeyCommandHandlers :
    IRequestHandler<CreatePartnerApiKeyCommand, Result<CreatePartnerApiKeyResultDto>>,
    IRequestHandler<ListPartnerApiKeysQuery, Result<IReadOnlyList<PartnerApiKeySummaryDto>>>,
    IRequestHandler<RevokePartnerApiKeyCommand, Result>
{
    private readonly IPartnerApiKeyRepository _apiKeyRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPartnerApiKeySecretGenerator _secretGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly PlatformBackofficeAuditRecorder _auditRecorder;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates handlers.</summary>
    public PartnerApiKeyCommandHandlers(
        IPartnerApiKeyRepository apiKeyRepository,
        ITenantRepository tenantRepository,
        IPartnerApiKeySecretGenerator secretGenerator,
        ICurrentUser currentUser,
        PlatformBackofficeAuditRecorder auditRecorder,
        IPlatformUnitOfWork unitOfWork)
    {
        _apiKeyRepository = apiKeyRepository;
        _tenantRepository = tenantRepository;
        _secretGenerator = secretGenerator;
        _currentUser = currentUser;
        _auditRecorder = auditRecorder;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<CreatePartnerApiKeyResultDto>> Handle(
        CreatePartnerApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure<CreatePartnerApiKeyResultDto>(PlatformErrorCodes.Tenant.NotFound);
        }

        var secret = _secretGenerator.GenerateSecret();
        var hash = PartnerApiKeyHasher.HashSecret(secret);
        var actor = _currentUser.UserId ?? string.Empty;
        var now = DateTimeOffset.UtcNow;
        var created = PartnerApiKey.Create(
            request.TenantId,
            request.PartnerName,
            PartnerApiKey.ExtractPrefix(secret),
            hash,
            PartnerApiKeyScopes.HealthRead,
            actor,
            now);
        if (created.IsFailure)
        {
            return Result.Failure<CreatePartnerApiKeyResultDto>(created.Error);
        }

        await _apiKeyRepository.AddAsync(created.Value, cancellationToken);
        await _auditRecorder.RecordAsync(
            request.TenantId,
            PlatformChangeActions.ApiKeyCreated,
            JsonSerializer.Serialize(new { partnerName = request.PartnerName, keyPrefix = created.Value.KeyPrefix }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatePartnerApiKeyResultDto(
            created.Value.Id,
            created.Value.PartnerName,
            created.Value.KeyPrefix,
            secret,
            created.Value.Scope,
            created.Value.CreatedAt));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PartnerApiKeySummaryDto>>> Handle(
        ListPartnerApiKeysQuery request,
        CancellationToken cancellationToken)
    {
        var keys = await _apiKeyRepository.ListByTenantAsync(request.TenantId, cancellationToken);
        var dtos = keys.Select(k => new PartnerApiKeySummaryDto(
            k.Id,
            k.PartnerName,
            k.KeyPrefix,
            k.Scope,
            k.IsActive,
            k.CreatedAt,
            k.RevokedAt)).ToList();
        return Result.Success<IReadOnlyList<PartnerApiKeySummaryDto>>(dtos);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(RevokePartnerApiKeyCommand request, CancellationToken cancellationToken)
    {
        var key = await _apiKeyRepository.GetByIdAsync(request.TenantId, request.KeyId, cancellationToken);
        if (key is null)
        {
            return Result.Failure(PlatformErrorCodes.ApiKey.NotFound);
        }

        var revoked = key.Revoke(DateTimeOffset.UtcNow);
        if (revoked.IsFailure)
        {
            return revoked;
        }

        await _auditRecorder.RecordAsync(
            request.TenantId,
            PlatformChangeActions.ApiKeyRevoked,
            JsonSerializer.Serialize(new { keyId = request.KeyId, keyPrefix = key.KeyPrefix }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
