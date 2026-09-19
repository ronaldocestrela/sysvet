namespace Clients.Infrastructure.Sales;

/// <summary>Fallback operator id for offline PDV when JWT user is not wired yet.</summary>
internal static class OfflineSalesDefaults
{
    internal static readonly Guid OperatorUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
}
