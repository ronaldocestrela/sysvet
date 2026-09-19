using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetRepository _petRepository;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICashRegisterRepository cashRegisterRepository,
        ITutorRepository tutorRepository,
        IPetRepository petRepository)
    {
        _orderRepository = orderRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _tutorRepository = tutorRepository;
        _petRepository = petRepository;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cashRegister = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (cashRegister == null || cashRegister.Status != CashRegisterStatus.Open)
        {
            return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Order.CashRegisterNotOpen);
        }

        if (request.TutorId.HasValue && request.TutorId != Guid.Empty)
        {
            var tutor = await _tutorRepository.GetByIdAsync(request.TutorId.Value, cancellationToken);
            if (tutor is null)
            {
                return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Order.TutorNotFound);
            }
        }

        if (request.PetId.HasValue && request.PetId != Guid.Empty)
        {
            var pet = await _petRepository.GetByIdAsync(request.PetId.Value, cancellationToken);
            if (pet is null)
            {
                return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Order.PetNotFound);
            }

            if (request.TutorId.HasValue && pet.TutorId != request.TutorId)
            {
                return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Order.PetTutorMismatch);
            }
        }

        var orderResult = Order.Create(
            request.CashRegisterId,
            request.TutorId,
            request.PetId,
            request.SourceQuoteId);
        if (!orderResult.IsSuccess)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value;

        foreach (var item in request.Items)
        {
            Result<bool> addResult = item.Kind switch
            {
                OrderItemKind.Service => order.AddServiceItem(item.ProductName, item.Quantity, item.UnitPrice),
                _ => order.AddProductItem(item.ProductId ?? Guid.Empty, item.ProductName, item.Quantity, item.UnitPrice)
            };

            if (!addResult.IsSuccess)
            {
                return Result.Failure<Guid>(addResult.Error);
            }
        }

        _orderRepository.Add(order);

        return Result.Success(order.Id);
    }
}
