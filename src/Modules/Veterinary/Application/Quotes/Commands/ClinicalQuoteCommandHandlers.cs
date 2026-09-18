using Core.Domain;
using Core.Domain.Auditing;
using MediatR;
using Veterinary.Application.Quotes.Dtos;
using Veterinary.Domain.Entities;
using VetErrors = Veterinary.Domain.ErrorCodes;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Quotes.Commands;

/// <summary>Creates draft clinical quotes.</summary>
public sealed class CreateClinicalQuoteCommandHandler : IRequestHandler<CreateClinicalQuoteCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClinicalQuoteRepository _quoteRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public CreateClinicalQuoteCommandHandler(
        IAppointmentRepository appointmentRepository,
        IClinicalQuoteRepository quoteRepository,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _appointmentRepository = appointmentRepository;
        _quoteRepository = quoteRepository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateClinicalQuoteCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<Guid>(VetErrors.Appointment.NotFound);
        }

        if (!appointment.IsEligibleForMedicalRecord())
        {
            return Result.Failure<Guid>(VetErrors.ClinicalQuote.AppointmentNotEligible);
        }

        var id = request.QuoteId == Guid.Empty ? Guid.NewGuid() : request.QuoteId;
        if (request.QuoteId != Guid.Empty)
        {
            var existing = await _quoteRepository.GetByIdAsync(id, cancellationToken);
            if (existing is not null)
            {
                return Result.Success(existing.Id);
            }
        }

        var created = ClinicalQuote.Create(
            id,
            appointment.Id,
            appointment.PetId,
            appointment.TutorId,
            _tenantContext.UserId);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var notes = created.Value.UpdateNotes(request.Notes);
            if (notes.IsFailure)
            {
                return Result.Failure<Guid>(notes.Error);
            }
        }

        await _quoteRepository.AddAsync(created.Value, cancellationToken);
        await ClinicalQuoteAuditHelper.LogAsync(_auditLogger, _tenantContext, id, "Create", "status=Draft", cancellationToken);
        return Result.Success(id);
    }
}

/// <summary>Replaces draft quote lines.</summary>
public sealed class ReplaceClinicalQuoteItemsCommandHandler : IRequestHandler<ReplaceClinicalQuoteItemsCommand, Result>
{
    private readonly IClinicalQuoteRepository _repository;

    public ReplaceClinicalQuoteItemsCommandHandler(IClinicalQuoteRepository repository) => _repository = repository;

    public async Task<Result> Handle(ReplaceClinicalQuoteItemsCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(VetErrors.ClinicalQuote.NotFound);
        }

        var lines = new List<(Guid, string, decimal, decimal, Veterinary.Domain.Enums.ClinicalQuoteItemKind, Guid?, int)>();
        foreach (var item in request.Items)
        {
            var kind = ClinicalQuoteMapper.ParseKind(item.Kind);
            if (kind.IsFailure)
            {
                return Result.Failure(kind.Error);
            }

            lines.Add((
                item.Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                kind.Value,
                item.ProductId,
                item.SortOrder));
        }

        var replace = await _repository.ReplaceDraftItemsAsync(request.QuoteId, lines, cancellationToken);
        if (replace.IsFailure)
        {
            return replace;
        }

        if (request.Notes is not null)
        {
            var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
            if (quote is null)
            {
                return Result.Failure(VetErrors.ClinicalQuote.NotFound);
            }

            var notes = quote.UpdateNotes(request.Notes);
            if (notes.IsFailure)
            {
                return notes;
            }

            _repository.Update(quote);
        }

        return Result.Success();
    }
}

/// <summary>Sends quote to tutor.</summary>
public sealed class SendClinicalQuoteCommandHandler : IRequestHandler<SendClinicalQuoteCommand, Result>
{
    private readonly IClinicalQuoteRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public SendClinicalQuoteCommandHandler(IClinicalQuoteRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SendClinicalQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Result.Failure(VetErrors.ClinicalQuote.NotFound);
        }

        var send = quote.Send();
        if (send.IsFailure)
        {
            return send;
        }

        _repository.Update(quote);
        await ClinicalQuoteAuditHelper.LogAsync(_auditLogger, _tenantContext, quote.Id, "Send", "status=Sent", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Approves quote and raises domain event for integration.</summary>
public sealed class ApproveClinicalQuoteCommandHandler : IRequestHandler<ApproveClinicalQuoteCommand, Result>
{
    private readonly IClinicalQuoteRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public ApproveClinicalQuoteCommandHandler(IClinicalQuoteRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ApproveClinicalQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Result.Failure(VetErrors.ClinicalQuote.NotFound);
        }

        var approve = quote.Approve();
        if (approve.IsFailure)
        {
            return approve;
        }

        _repository.Update(quote);
        await ClinicalQuoteAuditHelper.LogAsync(_auditLogger, _tenantContext, quote.Id, "Approve", "conversion=Pending", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Rejects sent quote.</summary>
public sealed class RejectClinicalQuoteCommandHandler : IRequestHandler<RejectClinicalQuoteCommand, Result>
{
    private readonly IClinicalQuoteRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public RejectClinicalQuoteCommandHandler(IClinicalQuoteRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(RejectClinicalQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Result.Failure(VetErrors.ClinicalQuote.NotFound);
        }

        var reject = quote.Reject();
        if (reject.IsFailure)
        {
            return reject;
        }

        _repository.Update(quote);
        await ClinicalQuoteAuditHelper.LogAsync(_auditLogger, _tenantContext, quote.Id, "Reject", "status=Rejected", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Gets quote detail.</summary>
public sealed class GetClinicalQuoteByIdQueryHandler : IRequestHandler<GetClinicalQuoteByIdQuery, Result<ClinicalQuoteDto>>
{
    private readonly IClinicalQuoteRepository _repository;

    public GetClinicalQuoteByIdQueryHandler(IClinicalQuoteRepository repository) => _repository = repository;

    public async Task<Result<ClinicalQuoteDto>> Handle(GetClinicalQuoteByIdQuery request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        return quote is null
            ? Result.Failure<ClinicalQuoteDto>(VetErrors.ClinicalQuote.NotFound)
            : Result.Success(ClinicalQuoteMapper.ToDto(quote));
    }
}

/// <summary>Lists quotes for appointment.</summary>
public sealed class ListClinicalQuotesByAppointmentQueryHandler : IRequestHandler<ListClinicalQuotesByAppointmentQuery, Result<IReadOnlyList<ClinicalQuoteListItemDto>>>
{
    private readonly IClinicalQuoteRepository _repository;

    public ListClinicalQuotesByAppointmentQueryHandler(IClinicalQuoteRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ClinicalQuoteListItemDto>>> Handle(ListClinicalQuotesByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var quotes = await _repository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        return Result.Success<IReadOnlyList<ClinicalQuoteListItemDto>>(quotes.Select(ClinicalQuoteMapper.ToListItem).ToList());
    }
}

/// <summary>Lists quotes for pet.</summary>
public sealed class ListClinicalQuotesByPetQueryHandler : IRequestHandler<ListClinicalQuotesByPetQuery, Result<IReadOnlyList<ClinicalQuoteListItemDto>>>
{
    private readonly IClinicalQuoteRepository _repository;

    public ListClinicalQuotesByPetQueryHandler(IClinicalQuoteRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ClinicalQuoteListItemDto>>> Handle(ListClinicalQuotesByPetQuery request, CancellationToken cancellationToken)
    {
        var quotes = await _repository.GetByPetIdAsync(request.PetId, cancellationToken);
        return Result.Success<IReadOnlyList<ClinicalQuoteListItemDto>>(quotes.Select(ClinicalQuoteMapper.ToListItem).ToList());
    }
}

/// <summary>Lists approved quotes pending sale conversion.</summary>
public sealed class ListPendingQuoteConversionsQueryHandler : IRequestHandler<ListPendingQuoteConversionsQuery, Result<IReadOnlyList<PendingQuoteConversionDto>>>
{
    private readonly IClinicalQuoteRepository _quoteRepository;
    private readonly IPetRepository _petRepository;
    private readonly ITutorRepository _tutorRepository;

    public ListPendingQuoteConversionsQueryHandler(
        IClinicalQuoteRepository quoteRepository,
        IPetRepository petRepository,
        ITutorRepository tutorRepository)
    {
        _quoteRepository = quoteRepository;
        _petRepository = petRepository;
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<IReadOnlyList<PendingQuoteConversionDto>>> Handle(ListPendingQuoteConversionsQuery request, CancellationToken cancellationToken)
    {
        var quotes = await _quoteRepository.ListPendingConversionsAsync(cancellationToken);
        var result = new List<PendingQuoteConversionDto>();

        foreach (var quote in quotes)
        {
            var pet = await _petRepository.GetByIdAsync(quote.PetId, cancellationToken);
            var tutor = await _tutorRepository.GetByIdAsync(quote.TutorId, cancellationToken);
            result.Add(new PendingQuoteConversionDto
            {
                QuoteId = quote.Id,
                AppointmentId = quote.AppointmentId,
                PetId = quote.PetId,
                PetName = pet?.Name ?? string.Empty,
                TutorId = quote.TutorId,
                TutorName = tutor?.Name ?? string.Empty,
                TotalAmount = quote.TotalAmount,
                DecidedAt = quote.DecidedAt,
                Items = quote.Items.OrderBy(i => i.SortOrder).Select(ClinicalQuoteMapper.ToItemDto).ToList()
            });
        }

        return Result.Success<IReadOnlyList<PendingQuoteConversionDto>>(result);
    }
}
