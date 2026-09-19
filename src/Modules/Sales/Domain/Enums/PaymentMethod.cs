namespace Sales.Domain.Enums;

/// <summary>Recorded payment instrument at checkout (TEF integration is Fase 6.3).</summary>
public enum PaymentMethod
{
    Cash = 0,
    DebitCard = 1,
    CreditCard = 2,
    Pix = 3
}
