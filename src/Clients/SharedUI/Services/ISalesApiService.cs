using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedUI.Services;

public interface ISalesApiService
{
    Task<Guid> OpenCashRegisterAsync(decimal openingBalance);
    Task<bool> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance);
    Task<CashRegisterDto?> GetOpenCashRegisterAsync();

    Task<Guid> CreateOrderAsync(Guid cashRegisterId, List<OrderItemDto> items);
    Task<bool> PayOrderAsync(Guid orderId);
}

public class CashRegisterDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
