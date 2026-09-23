using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Asaas payment row for a platform invoice (dbo).</summary>
public sealed class BillingCharge : Entity
{
    /// <summary>Parent invoice.</summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>Asaas payment id (pay_...).</summary>
    public string GatewayPaymentId { get; private set; } = string.Empty;

    /// <summary>Pix copy-paste payload when applicable.</summary>
    public string? PixCopyPaste { get; private set; }

    /// <summary>Boleto identification field when applicable.</summary>
    public string? BoletoIdentificationField { get; private set; }

    /// <summary>Navigation to invoice.</summary>
    public BillingInvoice? Invoice { get; private set; }

#pragma warning disable CS8618
    private BillingCharge()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates charge metadata returned by gateway.</summary>
    public static BillingCharge Create(
        Guid invoiceId,
        string gatewayPaymentId,
        string? pixCopyPaste,
        string? boletoLine)
    {
        return new BillingCharge
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            GatewayPaymentId = gatewayPaymentId,
            PixCopyPaste = pixCopyPaste,
            BoletoIdentificationField = boletoLine,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        };
    }
}
