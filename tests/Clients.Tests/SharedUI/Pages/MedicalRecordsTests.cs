using Bunit;
using Clients.Infrastructure.Crm;
using Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Pages;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Pages;

public class MedicalRecordsTests : BunitContext
{
    public MedicalRecordsTests()
    {
        Services.AddSingleton<IMedicalRecordStore, FakeMedicalRecordStore>();
        Services.AddSingleton<IClinicalStore, FakeClinicalStore>();
        Services.AddSingleton<IClinicalAttachmentService, FakeClinicalAttachmentService>();
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddSingleton<INavigationService, FakeNavigationService>();
    }

    [Fact]
    public void Should_Render_Medical_Records_Header()
    {
        var cut = Render<MedicalRecords>(parameters => parameters.Add(p => p.PetId, Guid.NewGuid()));
        cut.Find("h1").TextContent.MarkupMatches("Prontuário Médico");
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public void NavigateTo(string uri, bool forceLoad = false) { }
    }

    private sealed class FakeClinicalAttachmentService : IClinicalAttachmentService
    {
        public Task<Result<Guid>> UploadAsync(Guid appointmentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<string>> GetDownloadUrlAsync(Guid attachmentId)
            => Task.FromResult(Result.Success($"/api/v1/attachments/{attachmentId}/content"));
    }

    private sealed class FakeClinicalStore : IClinicalStore
    {
        public Task<Result<IReadOnlyList<ClinicalExamListItemDto>>> GetExamsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<ClinicalExamListItemDto>>(Array.Empty<ClinicalExamListItemDto>()));

        public Task<Result<Guid>> RequestExamAsync(Guid appointmentId, string name, string category, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<IReadOnlyList<IssuedPrescriptionListItemDto>>> GetPrescriptionsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<IssuedPrescriptionListItemDto>>(Array.Empty<IssuedPrescriptionListItemDto>()));

        public Task<Result<IReadOnlyList<ClinicalAttachmentListItemDto>>> GetAttachmentsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<ClinicalAttachmentListItemDto>>(Array.Empty<ClinicalAttachmentListItemDto>()));
    }

    private sealed class FakeMedicalRecordStore : IMedicalRecordStore
    {
        public Task<Result<IReadOnlyList<MedicalRecordTimelineItemDto>>> GetTimelineByPetAsync(Guid petId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<MedicalRecordTimelineItemDto>>(Array.Empty<MedicalRecordTimelineItemDto>()));

        public Task<Result<MedicalRecordDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<MedicalRecordDetailDto>(new Error("MedicalRecord.NotFound", "Not found")));

        public Task<Result<Guid>> GetOrCreateByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result> UpdateAnamnesisAsync(Guid medicalRecordId, string anamnesis, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> RecordVitalSignsAsync(Guid medicalRecordId, VitalSignsInputDto vitals, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result<Guid>> AddEvolutionNoteAsync(Guid medicalRecordId, string text, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result> SetDiagnosisAsync(Guid medicalRecordId, string diagnosis, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> SetConductAsync(Guid medicalRecordId, string conduct, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> FinalizeAsync(Guid medicalRecordId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }
}
