using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Core.Application.Privacy.Queries;

/// <summary>Exports tutor personal data for LGPD access/portability.</summary>
[AuthorizeRequest(AuthorizationPolicies.Admin, Permissions.PrivacyExport)]
public sealed record ExportTutorPersonalDataQuery(Guid TutorId) : IQuery<TutorPersonalDataExportDto>;
