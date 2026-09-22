using Automations.Application.Templates.Dtos;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Authorization;
using MediatR;

namespace Automations.Application.Templates.Commands;

/// <summary>
/// Creates a message template for a channel and business code.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record CreateMessageTemplateCommand(
    string Code,
    MessageChannel Channel,
    string Body,
    string? Subject = null,
    Guid IdempotencyKey = default) : ICommand<Guid>, IIdempotentCommand<Guid>;

/// <summary>
/// Updates template content and active flag.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record UpdateMessageTemplateCommand(
    Guid Id,
    string Body,
    string? Subject,
    bool IsActive,
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Lists all templates for the tenant.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record ListMessageTemplatesQuery : IQuery<IReadOnlyList<MessageTemplateDto>>;

/// <summary>
/// Gets a template by id.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record GetMessageTemplateByIdQuery(Guid Id) : IQuery<MessageTemplateDto>;

public sealed class CreateMessageTemplateCommandHandler : IRequestHandler<CreateMessageTemplateCommand, Result<Guid>>
{
    private readonly IMessageTemplateRepository _repository;

    public CreateMessageTemplateCommandHandler(IMessageTemplateRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateMessageTemplateCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.GetByCodeAndChannelAsync(request.Code, request.Channel, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(Automations.Domain.ErrorCodes.Template.DuplicateCode);
        }

        var created = MessageTemplate.Create(request.Code, request.Channel, request.Body, request.Subject);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _repository.Add(created.Value);
        return Result.Success(created.Value.Id);
    }
}

public sealed class UpdateMessageTemplateCommandHandler : IRequestHandler<UpdateMessageTemplateCommand, Result>
{
    private readonly IMessageTemplateRepository _repository;

    public UpdateMessageTemplateCommandHandler(IMessageTemplateRepository repository) => _repository = repository;

    public async Task<Result> Handle(UpdateMessageTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null)
        {
            return Result.Failure(Automations.Domain.ErrorCodes.Template.NotFound);
        }

        return template.Update(request.Body, request.Subject, request.IsActive);
    }
}

public sealed class ListMessageTemplatesQueryHandler : IRequestHandler<ListMessageTemplatesQuery, Result<IReadOnlyList<MessageTemplateDto>>>
{
    private readonly IMessageTemplateRepository _repository;

    public ListMessageTemplatesQueryHandler(IMessageTemplateRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<MessageTemplateDto>>> Handle(ListMessageTemplatesQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<MessageTemplateDto>>(list.Select(MessageTemplateMappings.ToDto).ToList());
    }
}

public sealed class GetMessageTemplateByIdQueryHandler : IRequestHandler<GetMessageTemplateByIdQuery, Result<MessageTemplateDto>>
{
    private readonly IMessageTemplateRepository _repository;

    public GetMessageTemplateByIdQueryHandler(IMessageTemplateRepository repository) => _repository = repository;

    public async Task<Result<MessageTemplateDto>> Handle(GetMessageTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return template is null
            ? Result.Failure<MessageTemplateDto>(Automations.Domain.ErrorCodes.Template.NotFound)
            : Result.Success(MessageTemplateMappings.ToDto(template));
    }
}

internal static class MessageTemplateMappings
{
    public static MessageTemplateDto ToDto(MessageTemplate t) =>
        new(t.Id, t.Code, t.Channel.ToString(), t.Subject, t.Body, t.IsActive);
}
