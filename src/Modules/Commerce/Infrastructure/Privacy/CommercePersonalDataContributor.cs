using Commerce.Domain.Repositories;
using Core.Application.Privacy;
using Core.Domain;

namespace Commerce.Infrastructure.Privacy;

/// <summary>Commerce module personal-data export and erasure for online orders.</summary>
public sealed class CommercePersonalDataContributor : IPersonalDataExportContributor, IPersonalDataErasureContributor
{
    private readonly IOnlineOrderRepository _orderRepository;

    /// <summary>Initializes repository dependency.</summary>
    public CommercePersonalDataContributor(IOnlineOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <inheritdoc />
    public string ModuleKey => "Commerce";

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, object?>> GetSlicesAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListByTutorIdAsync(tutorId, cancellationToken);
        return new Dictionary<string, object?>
        {
            ["onlineOrders"] = orders.Select(o => new Dictionary<string, object?>
            {
                ["id"] = o.Id,
                ["buyerName"] = o.BuyerName,
                ["buyerPhone"] = o.BuyerPhone,
                ["buyerEmail"] = o.BuyerEmail,
                ["status"] = o.Status.ToString()
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<Result> EraseForTutorAsync(PersonalDataErasureContext context, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListByTutorIdAsync(context.TutorId, cancellationToken);
        foreach (var order in orders)
        {
            order.ReplaceBuyerContact(context.TombstoneName, context.TombstonePhone, context.TombstoneEmail);
            _orderRepository.Update(order);
        }

        return Result.Success();
    }
}
