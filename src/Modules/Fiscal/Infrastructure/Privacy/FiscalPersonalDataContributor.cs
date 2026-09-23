using Core.Application.Privacy;
using Core.Domain;
using Fiscal.Domain.Repositories;

namespace Fiscal.Infrastructure.Privacy;

/// <summary>Fiscal module personal-data export and erasure for recipient copies.</summary>
public sealed class FiscalPersonalDataContributor : IPersonalDataExportContributor, IPersonalDataErasureContributor
{
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly ITutorRepository _tutorRepository;

    /// <summary>Initializes repository dependency.</summary>
    public FiscalPersonalDataContributor(
        IFiscalDocumentRepository documentRepository,
        ITutorRepository tutorRepository)
    {
        _documentRepository = documentRepository;
        _tutorRepository = tutorRepository;
    }

    /// <inheritdoc />
    public string ModuleKey => "Fiscal";

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, object?>> GetSlicesAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(tutorId, cancellationToken);
        if (tutor is null)
        {
            return new Dictionary<string, object?> { ["documents"] = Array.Empty<object>() };
        }

        var documents = await _documentRepository.ListByRecipientCpfAsync(tutor.Cpf.Number, cancellationToken);
        return new Dictionary<string, object?>
        {
            ["documents"] = documents.Select(d => new Dictionary<string, object?>
            {
                ["id"] = d.Id,
                ["recipientName"] = d.RecipientName,
                ["recipientCpf"] = d.RecipientCpf,
                ["documentType"] = d.DocumentType.ToString(),
                ["status"] = d.Status.ToString()
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<Result> EraseForTutorAsync(PersonalDataErasureContext context, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.ListByRecipientCpfAsync(context.OriginalCpf, cancellationToken);
        foreach (var document in documents)
        {
            document.ReplaceRecipientCopy(context.TombstoneName, context.TombstoneCpf);
            _documentRepository.Update(document);
        }

        return Result.Success();
    }
}
