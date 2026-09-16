using Core.Application.Messaging;

namespace Core.Application.Common;

/// <summary>
/// Commands that support idempotent execution via a client-supplied key.
/// </summary>
public interface IIdempotentCommandBase
{
    /// <summary>
    /// Client idempotency key; <see cref="Guid.Empty"/> skips idempotency handling.
    /// </summary>
    Guid IdempotencyKey { get; }
}

/// <summary>
/// Idempotent command with no typed success payload beyond <see cref="Core.Domain.Result"/>.
/// </summary>
public interface IIdempotentCommand : ICommand, IIdempotentCommandBase
{
}

/// <summary>
/// Idempotent command returning a typed success payload.
/// </summary>
/// <typeparam name="TResponse">Success payload type inside the result.</typeparam>
public interface IIdempotentCommand<TResponse> : ICommand<TResponse>, IIdempotentCommandBase
{
}
