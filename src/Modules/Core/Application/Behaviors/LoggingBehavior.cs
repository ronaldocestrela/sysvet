using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Behaviors;

/// <summary>
/// Intercepta as requisições (Commands/Queries) para logar a entrada, execução e saída.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Iniciando requisição {RequestName}", requestName);

        var response = await next();

        _logger.LogInformation("Requisição {RequestName} concluída", requestName);

        return response;
    }
}
