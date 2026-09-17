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

    /// <summary>
    /// Creates the handler.
    /// </summary>
    public PushSyncBatchCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Result<SyncPushResult>> Handle(PushSyncBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.Messages is null || request.Messages.Count == 0)
        {
            return Result.Success(new SyncPushResult());
        }

        var processed = new List<Guid>();

        foreach (var message in request.Messages.OrderBy(m => m.CreatedAt))
        {
            var command = SyncOutboxCommandMapper.MapToCommand(message);
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
                return Result.Failure(new Error("Sync.UnknownCommand", "Command type could not be dispatched."));
        }
    }
}
