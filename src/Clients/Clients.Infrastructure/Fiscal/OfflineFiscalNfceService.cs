using Clients.Infrastructure.Sync;
using Fiscal.Application.Documents;
using Fiscal.Application.Nfce;
using Fiscal.Domain.Enums;
using Fiscal.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Clients.Infrastructure.Fiscal;

/// <summary>Issues NFC-e contingency on local pay and enqueues transmission.</summary>
public sealed class OfflineFiscalNfceService
{
    private readonly OfflineDbContext _dbContext;

    public OfflineFiscalNfceService(OfflineDbContext dbContext) => _dbContext = dbContext;

    /// <summary>
    /// Attempts contingency NFC-e for product lines; never fails the sale.
    /// </summary>
    public async Task TryEnqueueContingencyNfceAsync(
        Order order,
        string? consumerCpf,
        Guid? tutorId,
        CancellationToken cancellationToken)
    {
        var issuer = await _dbContext.FiscalIssuerCache.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (issuer is null || !issuer.HasCertificate)
        {
            return;
        }

        var productLines = order.Items
            .Where(i => i.Kind is OrderItemKind.Product or OrderItemKind.Kit)
            .ToList();
        if (productLines.Count == 0)
        {
            return;
        }

        var sequence = await _dbContext.FiscalSequences.FirstOrDefaultAsync(cancellationToken);
        if (sequence is null)
        {
            sequence = new OfflineFiscalSequence { NfceSeries = issuer.NfceSeries };
            _dbContext.FiscalSequences.Add(sequence);
        }

        var number = (int)sequence.NextNfceNumber++;
        var series = sequence.NfceSeries;
        var recipientName = "Consumidor";
        string? recipientCpf = consumerCpf;
        if (tutorId is Guid tid)
        {
            var tutor = await _dbContext.Tutors.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tid, cancellationToken);
            if (tutor is not null)
            {
                recipientName = tutor.Name;
                recipientCpf ??= tutor.Cpf.Number;
            }
        }

        var lines = productLines.Select(i => new NfceContingencyLine(
            i.ProductName,
            i.Quantity,
            i.UnitPrice.Amount,
            "23091000")).ToList();

        var emission = _connectivityEmissionType();
        var built = NfceContingencyBuilder.Build(
            issuer.Cnpj,
            issuer.State,
            series,
            number,
            emission,
            recipientName,
            recipientCpf,
            lines);

        var documentId = Guid.NewGuid();
        _dbContext.FiscalDocuments.Add(new OfflineFiscalDocument
        {
            Id = documentId,
            OrderId = order.Id,
            Status = nameof(FiscalDocumentStatus.ContingencyIssued),
            AccessKey = built.AccessKey,
            QrCodeUrl = built.QrCodeUrl,
            SignedXml = built.SignedXml,
            NfeNumber = number,
            NfeSeries = series,
            RecipientName = recipientName,
            RecipientCpf = recipientCpf,
            EmissionType = emission.ToString(),
            UpdatedAt = DateTimeOffset.UtcNow
        });

        order.MarkFiscalPending();

        var transmitLines = productLines.Select(i => new TransmitNfceLineCommand(
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice.Amount,
            "23091000",
            FiscalTaxResolver.ResolveProductCfop(issuer.State, issuer.State),
            FiscalTaxResolver.DefaultCsosn,
            0)).ToList();

        var transmitOutboxId = Guid.NewGuid();
        var command = new TransmitNfceCommand
        {
            DocumentId = documentId,
            OrderId = order.Id,
            IdempotencyKey = transmitOutboxId,
            AccessKey = built.AccessKey,
            Number = number,
            Series = series,
            EmissionType = emission,
            SignedXml = built.SignedXml,
            QrCodeUrl = built.QrCodeUrl,
            RecipientName = recipientName,
            RecipientCpf = recipientCpf,
            RecipientUf = issuer.State,
            Lines = transmitLines
        };

        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = transmitOutboxId,
            Type = nameof(TransmitNfceCommand),
            Payload = System.Text.Json.JsonSerializer.Serialize(command),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static FiscalEmissionType _connectivityEmissionType() => FiscalEmissionType.ContingencyOffline;
}
