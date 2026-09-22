using Automations.Application.Jobs.Dtos;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Authorization;
using MediatR;

namespace Automations.Application.Jobs.Queries;

/// <summary>
/// Lists message jobs optionally filtered by status.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record ListMessageJobsQuery(MessageJobStatus? Status = null, int Take = 50) : IQuery<IReadOnlyList<MessageJobDto>>;

/// <summary>
/// Loads a single job with attempt logs.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record GetMessageJobByIdQuery(Guid Id) : IQuery<MessageJobDto>;

public sealed class ListMessageJobsQueryHandler : IRequestHandler<ListMessageJobsQuery, Result<IReadOnlyList<MessageJobDto>>>
{
    private readonly IMessageJobRepository _repository;

    public ListMessageJobsQueryHandler(IMessageJobRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<MessageJobDto>>> Handle(ListMessageJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _repository.ListAsync(request.Status, request.Take, cancellationToken);
        return Result.Success<IReadOnlyList<MessageJobDto>>(jobs.Select(MessageJobMappings.ToDto).ToList());
    }
}

public sealed class GetMessageJobByIdQueryHandler : IRequestHandler<GetMessageJobByIdQuery, Result<MessageJobDto>>
{
    private readonly IMessageJobRepository _repository;

    public GetMessageJobByIdQueryHandler(IMessageJobRepository repository) => _repository = repository;

    public async Task<Result<MessageJobDto>> Handle(GetMessageJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdWithLogsAsync(request.Id, cancellationToken);
        return job is null
            ? Result.Failure<MessageJobDto>(Automations.Domain.ErrorCodes.Job.NotFound)
            : Result.Success(MessageJobMappings.ToDto(job));
    }
}
