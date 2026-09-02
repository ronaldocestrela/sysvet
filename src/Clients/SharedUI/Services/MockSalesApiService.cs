using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedUI.Services;

public class MockSalesApiService : ISalesApiService
{
    private CashRegisterDto? _openRegister;

    public async Task<Guid> OpenCashRegisterAsync(decimal openingBalance)
    {
        await Task.Delay(300);
        _openRegister = new CashRegisterDto
        {
            Id = Guid.NewGuid(),
            Status = "Open",
            OpeningBalance = openingBalance,
            CurrentBalance = openingBalance
        };
        return _openRegister.Id;
    }

    public async Task<bool> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance)
    {
        await Task.Delay(300);
        if (_openRegister != null && _openRegister.Id == cashRegisterId)
        {
            _openRegister.Status = "Closed";
            _openRegister.CurrentBalance = actualClosingBalance;
            _openRegister = null;
            return true;
        }
        return false;
    }

    public async Task<CashRegisterDto?> GetOpenCashRegisterAsync()
    {
        await Task.Delay(100);
        return _openRegister;
    }

    public async Task<Guid> CreateOrderAsync(Guid cashRegisterId, List<OrderItemDto> items)
    {
        await Task.Delay(300);
        if (_openRegister != null)
        {
            foreach (var item in items)
            {
                _openRegister.CurrentBalance += (item.Quantity * item.UnitPrice);
            }
        }
        return Guid.NewGuid();
    }

    public async Task<bool> PayOrderAsync(Guid orderId)
    {
        await Task.Delay(300);
        return true;
    }
}
