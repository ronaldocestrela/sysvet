using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, ICashRegisterRepository cashRegisterRepository)
    {
        _orderRepository = orderRepository;
        _cashRegisterRepository = cashRegisterRepository;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cashRegister = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (cashRegister == null || cashRegister.Status != "Open")
        {
            return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Order.CashRegisterNotOpen);
        }

        var orderResult = Order.Create(request.CashRegisterId);
        if (!orderResult.IsSuccess) return Result.Failure<Guid>(orderResult.Error);

        var order = orderResult.Value;

        foreach (var item in request.Items)
        {
            var addResult = order.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
            if (!addResult.IsSuccess)
            {
                return Result.Failure<Guid>(addResult.Error);
            }
        }

        _orderRepository.Add(order);

        return Result.Success(order.Id);
    }
}
