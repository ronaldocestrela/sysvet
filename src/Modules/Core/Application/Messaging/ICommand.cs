using Core.Domain;
using MediatR;

namespace Core.Application.Messaging;

public interface ICommandBase { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase
{
}

public interface ICommand : IRequest<Result>, ICommandBase
{
}
