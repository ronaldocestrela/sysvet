using Core.Application.Common.Interfaces;
using Core.Application.Pets.Commands;
using Core.Application.Tutors.Commands;
using Core.Domain;
using MediatR;

namespace Core.Application.Sync;

/// <summary>
/// Applies client outbox commands in FIFO order, stopping on the first MediatR failure.
/// </summary>
public sealed class PushSyncBatchCommandHandler : IRequestHandler<PushSyncBatchCommand, Result<SyncPushResult>>
{
    private readonly IMediator _mediator;
    private readonly IEnumerable<ISyncPushHandler> _modulePushHandlers;
    private readonly ISyncPushObserver _syncPushObserver;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Creates the handler.
    /// </summary>
    public PushSyncBatchCommandHandler(
        IMediator mediator,
        IEnumerable<ISyncPushHandler> modulePushHandlers,
        ITenantContext tenantContext,
        ISyncPushObserver syncPushObserver)
    {
        _mediator = mediator;
        _modulePushHandlers = modulePushHandlers;
        _tenantContext = tenantContext;
        _syncPushObserver = syncPushObserver;
    }

    /// <inheritdoc />
    public async Task<Result<SyncPushResult>> Handle(PushSyncBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.Messages is null || request.Messages.Count == 0)
        {
            return Result.Success(new SyncPushResult());
        }

        var processed = new List<Guid>();

        // Preserve client FIFO order; do not re-sort by CreatedAt (Create+Pay often share the same tick).
        foreach (var message in request.Messages)
        {
            var command = SyncOutboxCommandMapper.MapToCommand(message);
            if (command is null)
            {
                command = TryMapModuleCommand(message);
            }

            if (command is null)
            {
                NotifyPushFailure(message.Id, "Sync.UnknownCommandType", true);
                return Result.Success(new SyncPushResult
                {
                    ProcessedIds = processed,
                    FailedMessageId = message.Id,
                    ErrorCode = "Sync.UnknownCommandType",
                    ErrorMessage = $"Unsupported outbox type '{message.Type}'.",
                    IsPermanentFailure = true
                });
            }

            var sendResult = await DispatchAsync(command, cancellationToken);
            if (sendResult.IsFailure)
            {
                var permanent = SyncOutboxCommandMapper.IsPermanentFailure(sendResult.Error);
                NotifyPushFailure(message.Id, sendResult.Error.Code, permanent);
                return Result.Success(new SyncPushResult
                {
                    ProcessedIds = processed,
                    FailedMessageId = message.Id,
                    ErrorCode = sendResult.Error.Code,
                    ErrorMessage = sendResult.Error.Message,
                    IsPermanentFailure = permanent
                });
            }

            processed.Add(message.Id);
        }

        return Result.Success(new SyncPushResult { ProcessedIds = processed });
    }

    private void NotifyPushFailure(Guid messageId, string errorCode, bool isPermanentFailure)
    {
        _syncPushObserver.OnPushFailure(_tenantContext.TenantId, messageId, errorCode, isPermanentFailure);
    }

    private object? TryMapModuleCommand(SyncOutboxMessageDto message)
    {
        foreach (var handler in _modulePushHandlers)
        {
            var command = handler.TryMapCommand(message);
            if (command is not null)
            {
                return command;
            }
        }

        return null;
    }

    private async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        switch (command)
        {
            case CreateTutorCommand c:
            {
                var result = await _mediator.Send(c, cancellationToken);
                return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
            }
            case UpdateTutorCommand c:
                return await _mediator.Send(c, cancellationToken);
            case DeleteTutorCommand c:
                return await _mediator.Send(c, cancellationToken);
            case CreatePetCommand c:
            {
                var result = await _mediator.Send(c, cancellationToken);
                return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
            }
            case UpdatePetCommand c:
                return await _mediator.Send(c, cancellationToken);
            case DeletePetCommand c:
                return await _mediator.Send(c, cancellationToken);
            default:
                foreach (var handler in _modulePushHandlers)
                {
                    var moduleResult = await handler.DispatchAsync(command, cancellationToken);
                    if (moduleResult.Error.Code != "Sync.HandlerMismatch")
                    {
                        return moduleResult;
                    }
                }

                return Result.Failure(new Error("Sync.UnknownCommand", "Command type could not be dispatched."));
        }
    }
}
