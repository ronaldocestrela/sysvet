using Core.Application.Common.Interfaces;
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
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IProductKitRepository _productKitRepository;
    private readonly IServicePackageRepository _servicePackageRepository;
    private readonly ICurrentUser _currentUser;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICashRegisterRepository cashRegisterRepository,
        ITutorRepository tutorRepository,
        IPetRepository petRepository,
        IAccessProfileRepository accessProfileRepository,
        IProductKitRepository productKitRepository,
        IServicePackageRepository servicePackageRepository,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _tutorRepository = tutorRepository;
        _petRepository = petRepository;
        _accessProfileRepository = accessProfileRepository;
        _productKitRepository = productKitRepository;
        _servicePackageRepository = servicePackageRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId is Guid orderId && orderId != Guid.Empty)
        {
            var existing = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (existing is not null)
            {
                return Result.Success(existing.Id);
            }
        }

        var discountValidation = await ValidateDiscountAsync(request.DiscountPercent, cancellationToken);
        if (discountValidation.IsFailure)
        {
            return Result.Failure<Guid>(discountValidation.Error);
        }

        var sellerResult = ResolveSellerUserId(request.SellerUserId);
        if (sellerResult.IsFailure)
        {
            return Result.Failure<Guid>(sellerResult.Error);
        }

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

        var sellerUserId = request.SellerUserId ?? sellerResult.Value;
        if (sellerUserId == Guid.Empty)
        {
            sellerUserId = cashRegister.OpenedByUserId;
        }

        var orderResult = request.OrderId is Guid clientOrderId && clientOrderId != Guid.Empty
            ? Order.Create(clientOrderId, request.CashRegisterId, sellerUserId, request.TutorId, request.PetId, request.SourceQuoteId)
            : Order.Create(request.CashRegisterId, sellerUserId, request.TutorId, request.PetId, request.SourceQuoteId);
        if (!orderResult.IsSuccess)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value;
        if (!string.IsNullOrWhiteSpace(request.ConsumerCpf))
        {
            var cpfResult = order.SetConsumerCpf(request.ConsumerCpf);
            if (cpfResult.IsFailure)
            {
                return Result.Failure<Guid>(cpfResult.Error);
            }
        }

        if (request.DiscountPercent > 0)
        {
            var discount = order.ApplyDiscount(request.DiscountPercent);
            if (!discount.IsSuccess)
            {
                return Result.Failure<Guid>(discount.Error);
            }
        }

        foreach (var item in request.Items)
        {
            var addResult = await AddItemAsync(order, item, cancellationToken);

            if (!addResult.IsSuccess)
            {
                return Result.Failure<Guid>(addResult.Error);
            }
        }

        _orderRepository.Add(order);

        return Result.Success(order.Id);
    }

    private async Task<Result<bool>> AddItemAsync(Order order, CreateOrderItemDto item, CancellationToken cancellationToken)
    {
        return item.Kind switch
        {
            OrderItemKind.Service => order.AddServiceItem(
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.PerformerUserId,
                item.PerformerRole),
            OrderItemKind.Kit => await AddKitAsync(order, item, cancellationToken),
            OrderItemKind.Package => await AddPackageAsync(order, item, cancellationToken),
            _ => order.AddProductItem(
                item.ProductId ?? Guid.Empty,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.PerformerUserId,
                item.PerformerRole)
        };
    }

    private async Task<Result<bool>> AddKitAsync(Order order, CreateOrderItemDto item, CancellationToken cancellationToken)
    {
        var kitId = item.CatalogOfferId ?? Guid.Empty;
        var kit = await _productKitRepository.GetByIdAsync(kitId, cancellationToken);
        if (kit is null || !kit.IsActive)
        {
            return Result.Failure<bool>(Sales.Domain.ErrorCodes.Kit.UnknownOffer);
        }

        var name = string.IsNullOrWhiteSpace(item.ProductName) ? kit.Name : item.ProductName;
        return order.AddKitItem(kitId, name, item.Quantity, item.UnitPrice);
    }

    private async Task<Result<bool>> AddPackageAsync(Order order, CreateOrderItemDto item, CancellationToken cancellationToken)
    {
        var packageId = item.CatalogOfferId ?? Guid.Empty;
        var package = await _servicePackageRepository.GetByIdAsync(packageId, cancellationToken);
        if (package is null || !package.IsActive)
        {
            return Result.Failure<bool>(Sales.Domain.ErrorCodes.Package.UnknownOffer);
        }

        var name = string.IsNullOrWhiteSpace(item.ProductName) ? package.Name : item.ProductName;
        return order.AddPackageItem(packageId, name, item.Quantity, item.UnitPrice);
    }

    private Result<Guid> ResolveSellerUserId(Guid? requestedSellerId)
    {
        if (requestedSellerId is Guid id && id != Guid.Empty)
        {
            return Result.Success(id);
        }

        if (_currentUser.IsAuthenticated && Guid.TryParse(_currentUser.UserId, out var userId))
        {
            return Result.Success(userId);
        }

        return Result.Success(Guid.Empty);
    }

    private async Task<Result> ValidateDiscountAsync(decimal discountPercent, CancellationToken cancellationToken)
    {
        if (discountPercent <= 0)
        {
            return Result.Success();
        }

        if (!_currentUser.IsAuthenticated || _currentUser.AccessProfileId == Guid.Empty)
        {
            return Result.Failure(Sales.Domain.ErrorCodes.Order.DiscountExceedsProfileLimit);
        }

        var profile = await _accessProfileRepository.GetByIdAsync(_currentUser.AccessProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(Sales.Domain.ErrorCodes.Order.DiscountExceedsProfileLimit);
        }

        if (discountPercent > profile.MaxDiscountPercent)
        {
            return Result.Failure(Sales.Domain.ErrorCodes.Order.DiscountExceedsProfileLimit);
        }

        return Result.Success();
    }
}
