using Bunit;
using Clients.Infrastructure.Crm;
using Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Pages;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Pages;

public class AppointmentsTests : BunitContext
{
    public AppointmentsTests()
    {
        Services.AddSingleton<IAppointmentStore, FakeAppointmentStore>();
        Services.AddSingleton<IPetStore, FakePetStoreForAppointments>();
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddSingleton<INavigationService, FakeNavigationService>();
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public void NavigateTo(string uri, bool forceLoad = false) { }
    }

    [Fact]
    public void Should_Render_Agenda_Header()
    {
        var cut = Render<Appointments>();
        cut.Find("h1").TextContent.MarkupMatches("Agenda de Consultas");
    }

    private sealed class FakeAppointmentStore : IAppointmentStore
    {
        public Task<Result<List<AppointmentListItemDto>>> GetDailyAsync(Guid? veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new List<AppointmentListItemDto>()));

        public Task<Result<Guid>> ScheduleAsync(ScheduleAppointmentRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result> ConfirmAsync(Guid appointmentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> StartAsync(Guid appointmentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> CancelAsync(Guid appointmentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class FakePetStoreForAppointments : IPetStore
    {
        public Task<Result<Clients.Infrastructure.Http.PagedResultDto<Clients.Infrastructure.Http.PetDto>>> ListAsync(int page, int pageSize, Guid? tutorId = null, CancellationToken cancellationToken = default)
        {
            var pageResult = new Clients.Infrastructure.Http.PagedResultDto<Clients.Infrastructure.Http.PetDto>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            };
            return Task.FromResult(Result.Success(pageResult));
        }

        public Task<Result<Clients.Infrastructure.Http.PetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<Clients.Infrastructure.Http.PetDto>(new Error("Pet.NotFound", "Not found")));

        public Task<Result<Guid>> CreateAsync(Clients.Infrastructure.Http.CreatePetRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result> UpdateAsync(Clients.Infrastructure.Http.UpdatePetRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }
}
