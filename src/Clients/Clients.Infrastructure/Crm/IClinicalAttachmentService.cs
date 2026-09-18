using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>Online-only upload/download for clinical attachment binaries.</summary>
public interface IClinicalAttachmentService
{
    Task<Result<Guid>> UploadAsync(Guid appointmentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);

    Task<Result<string>> GetDownloadUrlAsync(Guid attachmentId);
}
