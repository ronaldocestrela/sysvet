using Core.Domain;

namespace Fiscal.Domain;

/// <summary>Fiscal module error catalog.</summary>
public static class ErrorCodes
{
    public static class Issuer
    {
        public static readonly Error NotFound = new("Fiscal.Issuer.NotFound", "Issuer profile is not configured for this tenant.");
        public static readonly Error InvalidCnpj = new("Fiscal.Issuer.InvalidCnpj", "Invalid issuer CNPJ.");
        public static readonly Error CertificateMissing = new("Fiscal.Issuer.CertificateMissing", "Digital certificate is required before transmission.");
    }

    public static class Document
    {
        public static readonly Error NotFound = new("Fiscal.Document.NotFound", "Fiscal document was not found.");
        public static readonly Error InvalidTransition = new("Fiscal.Document.InvalidTransition", "Invalid status transition for this document.");
        public static readonly Error AlreadyAuthorized = new("Fiscal.Document.AlreadyAuthorized", "An authorized document already exists for this order and type.");
        public static readonly Error OrderNotPaid = new("Fiscal.Document.OrderNotPaid", "Fiscal documents can only be issued from paid orders.");
        public static readonly Error NoLines = new("Fiscal.Document.NoLines", "No fiscal lines were produced for this order.");
        public static readonly Error DestAddressRequired = new("Fiscal.DestAddressRequired", "Recipient address is required for NF-e.");
        public static readonly Error CancelWindowExpired = new("Fiscal.Document.CancelWindowExpired", "NF-e cancellation window has expired.");
        public static readonly Error CorrectionNotAllowed = new("Fiscal.Document.CorrectionNotAllowed", "Carta de correção is only allowed for authorized NF-e.");
        public static readonly Error CorrectionInvalidFields = new("Fiscal.Document.CorrectionInvalidFields", "Correction cannot change amounts, CFOP or recipient.");
    }

    public static class Nfse
    {
        public static readonly Error NationalEnvironmentUnavailable = new("Nfse.NationalEnvironmentUnavailable", "NFS-e Nacional (ADN) is not available for this municipality.");
    }

    public static class AccessKey
    {
        public static readonly Error Invalid = new("Fiscal.AccessKey.Invalid", "Access key must contain 44 digits.");
        public static readonly Error InvalidModel = new("Fiscal.AccessKey.InvalidModel", "Access key model must be 65 for NFC-e.");
    }

    public static class Cnpj
    {
        public static readonly Error Invalid = new("Fiscal.Cnpj.Invalid", "CNPJ must contain 14 digits.");
    }

    /// <summary>Validation errors for fiscal planning reports.</summary>
    public static class Report
    {
        public static readonly Error InvalidDateRange = new("Fiscal.Report.InvalidDateRange", "Start date cannot be after end date.");
        public static readonly Error RangeTooLarge = new("Fiscal.Report.RangeTooLarge", "Report range cannot exceed 366 days.");
        public static readonly Error ExportNotFullMonth = new("Fiscal.Report.ExportNotFullMonth", "Monthly export requires a full calendar month.");
    }
}
