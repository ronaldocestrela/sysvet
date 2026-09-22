using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Domain;
using MediatR;

namespace Automations.Application.Jobs.Commands;

/// <summary>
/// Persists a pending <see cref="MessageJob"/> when the idempotency key is new.
/// </summary>
public sealed class EnqueueMessageJobCommandHandler : IRequestHandler<EnqueueMessageJobCommand, Result<Guid>>
{
    private readonly IMessageJobRepository _jobRepository;
    private readonly ITenantContext _tenantContext;

    public EnqueueMessageJobCommandHandler(IMessageJobRepository jobRepository, ITenantContext tenantContext)
    {
        _jobRepository = jobRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(EnqueueMessageJobCommand request, CancellationToken cancellationToken)
    {
        if (request.Channel == MessageChannel.Sms)
        {
            return Result.Failure<Guid>(Automations.Domain.ErrorCodes.Channel.SmsNotSupported);
        }

        var idempotencyKey = request.IdempotencyKey == Guid.Empty
            ? string.Empty
            : request.IdempotencyKey.ToString("N");

        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await _jobRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (existing is not null)
            {
                return Result.Success(existing.Id);
            }
        }

        if (_tenantContext.TenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(new Error("MessageJob.InvalidTenant", "TenantId não resolvido para enfileirar job."));
        }

        var created = MessageJob.Enqueue(
            _tenantContext.TenantId,
            request.Channel,
            request.TemplateCode,
            request.PayloadJson,
            idempotencyKey,
            request.SourceType,
            request.SourceId);

        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _jobRepository.Add(created.Value);
        return Result.Success(created.Value.Id);
    }
}
