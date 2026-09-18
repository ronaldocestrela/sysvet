using Bunit;
using Clients.Infrastructure.Crm;
using Core.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Pages;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Pages;

public class HospitalizationsTests : BunitContext
{
    public HospitalizationsTests()
    {
        Services.AddSingleton<IHospitalizationStore, FakeHospitalizationStore>();
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddSingleton<INavigationService, FakeNavigationService>();
    }

    [Fact]
    public void Should_Render_Execution_Map_Header()
    {
        var cut = Render<Hospitalizations>();
        cut.Find("h1").TextContent.MarkupMatches("Mapa de execução");
    }

    [Fact]
    public void Should_Show_Ward_And_Bed_From_Store()
    {
        var cut = Render<Hospitalizations>();
        cut.WaitForAssertion(() => cut.Markup.Contains("UTI"));
        cut.Markup.Contains("L1").Should().BeTrue();
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public void NavigateTo(string uri, bool forceLoad = false) { }
    }

    private sealed class FakeHospitalizationStore : IHospitalizationStore
    {
        public Task<Result<ExecutionMapViewModel>> GetExecutionMapAsync(DateOnly? date = null, CancellationToken cancellationToken = default)
        {
            var map = new ExecutionMapViewModel
            {
                Date = date ?? DateOnly.FromDateTime(DateTime.UtcNow),
                Wards =
                [
                    new ExecutionMapWardViewModel
                    {
                        WardName = "UTI",
                        Beds =
                        [
                            new ExecutionMapBedViewModel
                            {
                                BedCode = "L1",
                                HospitalizationId = Guid.NewGuid(),
                                PetName = "Rex",
                                Administrations =
                                [
                                    new ExecutionMapAdministrationViewModel
                                    {
                                        MedicationName = "Dipyrone",
                                        Status = "Pending"
                                    }
                                ]
                            }
                        ]
                    }
                ]
            };
            return Task.FromResult(Result.Success(map));
        }

        public Task<Result<IReadOnlyList<HospitalizationListViewModel>>> ListActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success<IReadOnlyList<HospitalizationListViewModel>>(Array.Empty<HospitalizationListViewModel>()));

        public Task<Result<Guid>> AdmitAsync(Guid petId, Guid veterinarianId, Guid bedId, string reason, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result> DischargeAsync(Guid hospitalizationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());

        public Task<Result> AdministerAsync(Guid hospitalizationId, Guid administrationId, string? notes, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());

        public Task<Result> SkipAsync(Guid hospitalizationId, Guid administrationId, string? notes, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());

        public Task<Result<Guid>> CreateMedicationOrderAsync(
            Guid hospitalizationId,
            string medicationName,
            string dose,
            string route,
            IReadOnlyList<TimeOnly> dailyTimes,
            DateOnly startsOn,
            DateOnly endsOn,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<HospitalizationDetailViewModel>> GetDetailAsync(Guid hospitalizationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(new HospitalizationDetailViewModel { Id = hospitalizationId, PetName = "Rex" }));
    }
}
