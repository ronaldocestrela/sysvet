using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;
using System.Reflection;

namespace Core.Application.Behaviors;

/// <summary>
/// Enforces <see cref="AuthorizeRequestAttribute"/> on MediatR requests before validation and handlers run.
/// </summary>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionChecker _permissionChecker;

    public AuthorizationBehavior(ICurrentUser currentUser, IPermissionChecker permissionChecker)
    {
        _currentUser = currentUser;
        _permissionChecker = permissionChecker;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var attribute = request.GetType().GetCustomAttribute<AuthorizeRequestAttribute>();
        if (attribute is null)
        {
            return await next();
        }

        if (!_currentUser.IsAuthenticated)
        {
            return CreateFailure<TResponse>(ErrorCodes.Authorization.Unauthorized);
        }

        if (!await _currentUser.IsInPolicyAsync(attribute.Policy, cancellationToken))
        {
            return CreateFailure<TResponse>(ErrorCodes.Authorization.Forbidden);
        }

        if (!string.IsNullOrEmpty(attribute.Permission)
            && !await _permissionChecker.HasPermissionAsync(attribute.Permission, cancellationToken))
        {
            return CreateFailure<TResponse>(ErrorCodes.Authorization.Forbidden);
        }

        return await next();
    }

    private static TResult CreateFailure<TResult>(Error error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Result.Failure(error);
        }

        var valueType = typeof(TResult).GenericTypeArguments[0];
        var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);
        return (TResult)failureMethod.Invoke(null, [error])!;
    }
}
