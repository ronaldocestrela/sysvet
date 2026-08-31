using Core.Domain;
using Core.Application.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommandBase)
        {
            return await next();
        }

        var response = await next();

        // Check if TResponse is a Result, and only commit if it is successful.
        if (response is Result result && !result.IsSuccess)
        {
            return response;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
