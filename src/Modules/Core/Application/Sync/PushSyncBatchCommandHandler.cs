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

    /// <summary>
    /// Creates the handler.
    /// </summary>
    public PushSyncBatchCommandHandler(IMediator mediator, IEnumerable<ISyncPushHandler> modulePushHandlers)
    {
        _mediator = mediator;
        _modulePushHandlers = modulePushHandlers;
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
                return Result.Success(new SyncPushResult
                {
                    ProcessedIds = processed,
                    FailedMessageId = message.Id,
                    ErrorCode = sendResult.Error.Code,
                    ErrorMessage = sendResult.Error.Message,
                    IsPermanentFailure = SyncOutboxCommandMapper.IsPermanentFailure(sendResult.Error)
                });
            }

            processed.Add(message.Id);
        }

        return Result.Success(new SyncPushResult { ProcessedIds = processed });
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
