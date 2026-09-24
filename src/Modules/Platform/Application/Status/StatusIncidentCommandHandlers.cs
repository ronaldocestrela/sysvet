using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Application.Status;

/// <summary>Creates and resolves status incidents.</summary>
public sealed class CreateStatusIncidentCommandHandler : IRequestHandler<CreateStatusIncidentCommand, Result<StatusIncidentDto>>
{
    private readonly IStatusIncidentRepository _repository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public CreateStatusIncidentCommandHandler(IStatusIncidentRepository repository, IPlatformUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<StatusIncidentDto>> Handle(CreateStatusIncidentCommand request, CancellationToken cancellationToken)
    {
        var created = StatusIncident.Open(request.Title, request.Impact, request.Components, DateTimeOffset.UtcNow);
        if (created.IsFailure)
        {
            return Result.Failure<StatusIncidentDto>(created.Error);
        }

        await _repository.AddAsync(created.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(created.Value));
    }

    internal static StatusIncidentDto Map(StatusIncident incident) =>
        new(incident.Id, incident.Title, incident.Impact, incident.Components, incident.StartedAt, incident.ResolvedAt);
}

/// <summary>Resolves an incident.</summary>
public sealed class ResolveStatusIncidentCommandHandler : IRequestHandler<ResolveStatusIncidentCommand, Result<StatusIncidentDto>>
{
    private readonly IStatusIncidentRepository _repository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public ResolveStatusIncidentCommandHandler(IStatusIncidentRepository repository, IPlatformUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<StatusIncidentDto>> Handle(ResolveStatusIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdAsync(request.IncidentId, cancellationToken);
        if (incident is null)
        {
            return Result.Failure<StatusIncidentDto>(Platform.Domain.ErrorCodes.StatusIncident.NotFound);
        }

        var resolved = incident.Resolve(DateTimeOffset.UtcNow);
        if (resolved.IsFailure)
        {
            return Result.Failure<StatusIncidentDto>(resolved.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(CreateStatusIncidentCommandHandler.Map(incident));
    }
}
