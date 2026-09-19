using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Sales.Domain.Enums;

namespace Sales.Application.Orders.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesWrite)]
public class PayOrderCommand : ICommand<bool>, IIdempotentCommand<bool>
{
    public Guid OrderId { get; set; }
    public List<PayOrderPaymentDto> Payments { get; set; } = new();
    public Guid IdempotencyKey { get; set; }
}

public class PayOrderPaymentDto
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? Nsu { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? Provider { get; set; }
    public string? TerminalId { get; set; }
    public string? Brand { get; set; }
    public int Installments { get; set; } = 1;
}
