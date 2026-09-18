using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using MediatR;
using Veterinary.Application.Quotes.Dtos;

namespace Veterinary.Application.Quotes.Commands;

/// <summary>Line input for quote item replace operations.</summary>
public sealed record ClinicalQuoteLineInput(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string Kind,
    Guid? ProductId,
    int SortOrder);

/// <summary>Creates a draft clinical quote for an appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesWrite)]
public sealed record CreateClinicalQuoteCommand(Guid AppointmentId, string? Notes, Guid QuoteId = default, Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Replaces draft quote lines and optional notes.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesWrite)]
public sealed record ReplaceClinicalQuoteItemsCommand(
    Guid QuoteId,
    IReadOnlyList<ClinicalQuoteLineInput> Items,
    string? Notes,
    Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Sends a draft quote to the tutor.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesWrite)]
public sealed record SendClinicalQuoteCommand(Guid QuoteId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Records tutor approval and queues PDV conversion.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesWrite)]
public sealed record ApproveClinicalQuoteCommand(Guid QuoteId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Records tutor rejection.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesWrite)]
public sealed record RejectClinicalQuoteCommand(Guid QuoteId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Gets a quote by identifier.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesRead)]
public sealed record GetClinicalQuoteByIdQuery(Guid QuoteId) : IQuery<ClinicalQuoteDto>;

/// <summary>Lists quotes for an appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesRead)]
public sealed record ListClinicalQuotesByAppointmentQuery(Guid AppointmentId) : IQuery<IReadOnlyList<ClinicalQuoteListItemDto>>;

/// <summary>Lists quotes for a pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesRead)]
public sealed record ListClinicalQuotesByPetQuery(Guid PetId) : IQuery<IReadOnlyList<ClinicalQuoteListItemDto>>;

/// <summary>Lists approved quotes pending PDV conversion.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicalQuotesRead)]
public sealed record ListPendingQuoteConversionsQuery : IQuery<IReadOnlyList<PendingQuoteConversionDto>>;
