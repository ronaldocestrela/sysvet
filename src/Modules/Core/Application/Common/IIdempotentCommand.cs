using MediatR;

namespace Core.Application.Common;

/// <summary>
/// Indica que o comando suporta execução idempotente.
/// </summary>
public interface IIdempotentCommand<TResponse> : IRequest<TResponse>
{
    Guid IdempotencyKey { get; }
}
