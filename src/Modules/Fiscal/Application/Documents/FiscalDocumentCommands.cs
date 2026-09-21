using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Fiscal.Domain.Enums;

namespace Fiscal.Application.Documents;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.FiscalWrite)]
public sealed record IssueFromOrderCommand(Guid OrderId) : ICommand<IReadOnlyList<Guid>>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalRead)]
public sealed record ListFiscalDocumentsQuery(Guid? OrderId, FiscalDocumentStatus? Status) : IQuery<IReadOnlyList<FiscalDocumentDto>>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalRead)]
public sealed record GetFiscalDocumentByIdQuery(Guid Id) : IQuery<FiscalDocumentDetailDto?>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.FiscalWrite)]
public sealed record CancelFiscalDocumentCommand(Guid DocumentId, string Justification) : ICommand<bool>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.FiscalWrite)]
public sealed record IssueCorrectionLetterCommand(Guid DocumentId, string CorrectionText) : ICommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalRead)]
public sealed record DownloadFiscalXmlQuery(Guid DocumentId) : IQuery<FiscalFileDownloadDto?>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalRead)]
public sealed record DownloadFiscalDanfeQuery(Guid DocumentId) : IQuery<FiscalFileDownloadDto?>;

[AuthorizeRequest(AuthorizationPolicies.Admin, Permissions.FiscalWrite)]
public sealed record UploadIssuerCertificateCommand(byte[] PfxBytes, string Password) : ICommand<bool>;
