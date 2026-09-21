using System.Security.Cryptography;
using System.Text;

namespace Finance.Application.Common;

/// <summary>
/// Stable correlation identifiers for sale payment slices.
/// </summary>
public static class PaymentCorrelation
{
    public static Guid ForOrderPayment(Guid orderId, int index, string method, decimal amount, string? nsu)
    {
        var payload = $"{orderId:N}:{index}:{method}:{amount:F2}:{nsu ?? ""}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return new Guid(hash.AsSpan(0, 16));
    }
}
