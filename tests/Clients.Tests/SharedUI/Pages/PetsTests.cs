using Bunit;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Http;
using Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Pages;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Pages;

public class PetsTests : BunitContext
{
    public PetsTests()
    {
        Services.AddSingleton<IPetStore, FakePetStore>();
        Services.AddSingleton<ITutorStore, FakeTutorStore>();
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddSingleton<INavigationService, FakeNavigationService>();
    }

    [Fact]
    public void Should_Render_Pets_Header_And_New_Button()
    {
        var cut = Render<Pets>();

        cut.Find("h1").TextContent.MarkupMatches("Pets");
        cut.Find("button.btn-primary").TextContent.MarkupMatches("Novo Pet");
    }

    [Fact]
    public void Should_Open_Modal_When_New_Button_Clicked()
    {
        var cut = Render<Pets>();

        cut.Find("button.btn-primary").Click();
        cut.WaitForAssertion(() =>
        {
            cut.Find(".modal-header h3").TextContent.MarkupMatches("Novo Pet");
        });
    }

    private sealed class FakePetStore : IPetStore
    {
        public Task<Result<PagedResultDto<PetDto>>> ListAsync(int page, int pageSize, Guid? tutorId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new PagedResultDto<PetDto>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            }));

        public Task<Result<PetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<PetDto>(ErrorCodes.Pet.NotFound));

        public Task<Result<Guid>> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(request.Id == Guid.Empty ? Guid.NewGuid() : request.Id));

        public Task<Result> UpdateAsync(UpdatePetRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public string? LastUri { get; private set; }
        public void NavigateTo(string uri, bool forceLoad = false) => LastUri = uri;
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
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result> UpdateAsync(UpdateTutorRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }
}
