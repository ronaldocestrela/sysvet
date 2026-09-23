using Core.Application.Privacy;
using Core.Domain;
using Sales.Domain.Repositories;

namespace Sales.Infrastructure.Privacy;

/// <summary>Sales module personal-data export and erasure for tutor-linked orders.</summary>
public sealed class SalesPersonalDataContributor : IPersonalDataExportContributor, IPersonalDataErasureContributor
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>Initializes repository dependency.</summary>
    public SalesPersonalDataContributor(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <inheritdoc />
    public string ModuleKey => "Sales";

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, object?>> GetSlicesAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListByTutorIdAsync(tutorId, cancellationToken);
        return new Dictionary<string, object?>
        {
            ["orders"] = orders.Select(o => new Dictionary<string, object?>
            {
                ["id"] = o.Id,
                ["consumerCpf"] = o.ConsumerCpf,
                ["status"] = o.Status.ToString(),
                ["paidAt"] = o.PaidAt
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<Result> EraseForTutorAsync(PersonalDataErasureContext context, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListByTutorIdAsync(context.TutorId, cancellationToken);
        foreach (var order in orders)
        {
            order.ReplaceConsumerCpfCopy(context.TombstoneCpf);
            _orderRepository.Update(order);
        }

        return Result.Success();
    }
}
