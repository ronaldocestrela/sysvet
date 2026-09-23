using Core.Application.Messaging;
using Core.Application.Storage;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Billing;

/// <summary>Handlers for VetNexus NFS-e emission (9.6).</summary>
public sealed class SaasNfseCommandHandlers :
    IRequestHandler<IssueSaasNfseForInvoiceCommand, Result>,
    IRequestHandler<RetrySaasNfseForInvoiceCommand, Result<SaasNfseDto>>
{
    private readonly IBillingInvoiceRepository _invoiceRepository;
    private readonly ISaasServiceInvoiceRepository _nfseRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ISaasNfseGateway _gateway;
    private readonly IBlobStorage _blobStorage;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates handlers.</summary>
    public SaasNfseCommandHandlers(
        IBillingInvoiceRepository invoiceRepository,
        ISaasServiceInvoiceRepository nfseRepository,
        IBranchRepository branchRepository,
        ISaasNfseGateway gateway,
        IBlobStorage blobStorage,
        IPlatformUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _nfseRepository = nfseRepository;
        _branchRepository = branchRepository;
        _gateway = gateway;
        _blobStorage = blobStorage;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(IssueSaasNfseForInvoiceCommand request, CancellationToken cancellationToken)
    {
        var issue = await IssueInternalAsync(request.InvoiceId, request.ForceRetry, cancellationToken);
        return issue.IsFailure ? issue : Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<SaasNfseDto>> Handle(RetrySaasNfseForInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice is null || invoice.TenantId != request.TenantId)
        {
            return Result.Failure<SaasNfseDto>(PlatformErrorCodes.Billing.InvoiceNotFound);
        }

        var issue = await IssueInternalAsync(request.InvoiceId, forceRetry: true, cancellationToken);
        if (issue.IsFailure)
        {
            return Result.Failure<SaasNfseDto>(issue.Error);
        }

        var row = await _nfseRepository.GetByBillingInvoiceIdAsync(request.InvoiceId, cancellationToken);
        return row is null
            ? Result.Failure<SaasNfseDto>(PlatformErrorCodes.Nfse.InvalidGatewayResponse)
            : Result.Success(Map(row));
    }

    private async Task<Result> IssueInternalAsync(Guid invoiceId, bool forceRetry, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result.Failure(PlatformErrorCodes.Billing.InvoiceNotFound);
        }

        if (invoice.Status != BillingInvoiceStatus.Paid)
        {
            return Result.Failure(PlatformErrorCodes.Nfse.InvoiceNotPaid);
        }

        if (invoice.Amount <= 0)
        {
            return Result.Success();
        }

        var existing = await _nfseRepository.GetByBillingInvoiceIdAsync(invoiceId, cancellationToken);
        if (existing?.Status == SaasServiceInvoiceStatus.Authorized)
        {
            return Result.Success();
        }

        if (existing?.Status == SaasServiceInvoiceStatus.Failed && !forceRetry)
        {
            return Result.Success();
        }

        var headquarters = await _branchRepository.GetHeadquartersAsync(invoice.TenantId, cancellationToken);
        if (headquarters is null)
        {
            return Result.Failure(PlatformErrorCodes.Nfse.HeadquartersMissing);
        }

        SaasServiceInvoice row;
        if (existing is null)
        {
            var created = SaasServiceInvoice.CreatePending(
                invoice.Id,
                invoice.TenantId,
                invoice.Amount,
                headquarters.Cnpj,
                headquarters.LegalName);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            row = created.Value;
            await _nfseRepository.AddAsync(row, cancellationToken);
        }
        else
        {
            row = existing;
            if (row.Status == SaasServiceInvoiceStatus.Failed)
            {
                var reset = row.ResetForRetry();
                if (reset.IsFailure)
                {
                    return reset;
                }
            }
        }

        var gatewayResult = await _gateway.AuthorizeAsync(new SaasNfseAuthorizationRequest
        {
            BillingInvoiceId = invoice.Id,
            Amount = invoice.Amount,
            RecipientCnpj = headquarters.Cnpj,
            RecipientLegalName = headquarters.LegalName,
            RecipientPostalCode = headquarters.PostalCode,
            RecipientStreet = headquarters.Street,
            RecipientStreetNumber = headquarters.StreetNumber,
            RecipientDistrict = headquarters.District,
            RecipientCity = headquarters.City,
            RecipientStateCode = headquarters.StateCode,
            RecipientIbgeCode = headquarters.IbgeCode
        }, cancellationToken);

        if (gatewayResult.IsFailure)
        {
            row.MarkFailed(gatewayResult.Error.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        var payload = gatewayResult.Value;
        if (!payload.Success)
        {
            row.MarkFailed(payload.FailureReason ?? "NFS-e não autorizada.");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        var blobKey = $"platform/nfse/{invoice.Id}.xml";
        var xml = payload.Xml ?? $"<NFSe invoice=\"{invoice.Id}\"/>";
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var upload = await _blobStorage.PutAsync(blobKey, stream, "application/xml", cancellationToken);
        if (upload.IsFailure)
        {
            row.MarkFailed(upload.Error.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        var authorized = row.MarkAuthorized(
            payload.NfseNumber ?? "0",
            payload.AccessKey ?? $"NFSE-{invoice.Id:N}",
            blobKey);
        if (authorized.IsFailure)
        {
            return authorized;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static SaasNfseDto Map(SaasServiceInvoice row) =>
        new(
            row.Id,
            row.BillingInvoiceId,
            row.TenantId,
            row.Amount,
            (SaasServiceInvoiceStatusDto)row.Status,
            row.NfseNumber,
            row.AccessKey,
            row.FailureReason);
}
