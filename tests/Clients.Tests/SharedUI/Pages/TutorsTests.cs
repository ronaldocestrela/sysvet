using Bunit;
using Clients.Infrastructure.Automations;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Sync;
using Clients.Tests.Fakes;
using Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Pages;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Pages;

public class TutorsTests : BunitContext
{
    public TutorsTests()
    {
        Services.AddSingleton<ITutorStore, FakeTutorStore>();
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddSingleton<IConnectivityService>(_ => new FakeConnectivityService(ConnectivityStatus.Offline));
        Services.AddSingleton(_ => new AutomationsApiService(
            new ApiClient(new HttpClient { BaseAddress = new Uri("http://localhost/") }),
            new OfflineSyncConnectivity()));
    }

    [Fact]
    public void Should_Render_Tutors_Header_And_New_Button()
    {
        var cut = Render<Tutors>();

        cut.Find("h1").TextContent.MarkupMatches("Tutores");
        cut.Find("button.btn-primary").TextContent.MarkupMatches("Novo Tutor");
    }

    [Fact]
    public void Should_Open_Modal_When_New_Button_Clicked()
    {
        var cut = Render<Tutors>();

        cut.Find("button.btn-primary").Click();
        cut.WaitForAssertion(() =>
        {
            cut.Find(".modal-header h3").TextContent.MarkupMatches("Novo Tutor");
        });
    }

    private sealed class FakeTutorStore : ITutorStore
    {
        public Task<Result<PagedResultDto<TutorDto>>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new PagedResultDto<TutorDto>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            }));

        public Task<Result<TutorDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<TutorDto>(ErrorCodes.Tutor.NotFound));

        public Task<Result<Guid>> CreateAsync(CreateTutorRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(request.Id == Guid.Empty ? Guid.NewGuid() : request.Id));

        public Task<Result> UpdateAsync(UpdateTutorRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class OfflineSyncConnectivity : ISyncConnectivity
    {
        public bool IsOnline => false;
        public event EventHandler? OnlineStateChanged
        {
            add { }
            remove { }
        }
        public void SetSyncing(bool isSyncing) { }
    }
}
