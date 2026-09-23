using Clients.Infrastructure.Billing;
using Core.Domain;

namespace Clients.Tests.Fakes;

/// <summary>Unlocked billing standing for layout tests that do not exercise dunning.</summary>
public sealed class FakeClinicBillingApi : IClinicBillingApi
{
    public Task<Result<ClinicBillingStandingResponse>> GetStandingAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new ClinicBillingStandingResponse(1, false, null, null, null, null, null, null)));

    public Task<Result<PayClinicBillingResponse>> PayOutstandingAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Failure<PayClinicBillingResponse>(new Error("Billing.NotUsed", "Pagamento não exercitado neste teste.")));
}
