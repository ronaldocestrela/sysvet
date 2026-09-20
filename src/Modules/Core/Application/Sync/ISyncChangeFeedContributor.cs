namespace Core.Application.Sync;

/// <summary>
/// Supplies module rows for sync pull (appointments, slots, etc.).
/// </summary>
public interface ISyncChangeFeedContributor
{
    /// <summary>Reads changes after <paramref name="since"/> up to <paramref name="take"/>.</summary>
    Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken);
}

/// <summary>Partial pull page from one module contributor.</summary>
public sealed class SyncContributorChanges
{
    public IReadOnlyList<SyncAppointmentDto> Appointments { get; init; } = Array.Empty<SyncAppointmentDto>();
    public IReadOnlyList<SyncScheduleSlotDto> ScheduleSlots { get; init; } = Array.Empty<SyncScheduleSlotDto>();
    public IReadOnlyList<SyncMedicalRecordDto> MedicalRecords { get; init; } = Array.Empty<SyncMedicalRecordDto>();
    public IReadOnlyList<SyncPrescriptionTemplateDto> PrescriptionTemplates { get; init; } = Array.Empty<SyncPrescriptionTemplateDto>();
    public IReadOnlyList<SyncIssuedPrescriptionDto> IssuedPrescriptions { get; init; } = Array.Empty<SyncIssuedPrescriptionDto>();
    public IReadOnlyList<SyncClinicalExamDto> ClinicalExams { get; init; } = Array.Empty<SyncClinicalExamDto>();
    public IReadOnlyList<SyncClinicalAttachmentDto> ClinicalAttachments { get; init; } = Array.Empty<SyncClinicalAttachmentDto>();
    public IReadOnlyList<SyncVaccineProtocolDto> VaccineProtocols { get; init; } = Array.Empty<SyncVaccineProtocolDto>();
    public IReadOnlyList<SyncVaccineDoseDto> VaccineDoses { get; init; } = Array.Empty<SyncVaccineDoseDto>();
    public IReadOnlyList<SyncClinicalQuoteDto> ClinicalQuotes { get; init; } = Array.Empty<SyncClinicalQuoteDto>();
    public IReadOnlyList<SyncWardUnitDto> WardUnits { get; init; } = Array.Empty<SyncWardUnitDto>();
    public IReadOnlyList<SyncHospitalizationDto> Hospitalizations { get; init; } = Array.Empty<SyncHospitalizationDto>();
    public IReadOnlyList<SyncInventoryProductDto> InventoryProducts { get; init; } = Array.Empty<SyncInventoryProductDto>();
    public IReadOnlyList<SyncInventoryProductLotDto> InventoryProductLots { get; init; } = Array.Empty<SyncInventoryProductLotDto>();
    public IReadOnlyList<SyncInventorySupplierDto> InventorySuppliers { get; init; } = Array.Empty<SyncInventorySupplierDto>();
    public IReadOnlyList<SyncInventoryStockMovementDto> InventoryStockMovements { get; init; } = Array.Empty<SyncInventoryStockMovementDto>();
    public IReadOnlyList<SyncSalesCashRegisterDto> SalesCashRegisters { get; init; } = Array.Empty<SyncSalesCashRegisterDto>();
    public IReadOnlyList<SyncSalesOrderDto> SalesOrders { get; init; } = Array.Empty<SyncSalesOrderDto>();
    public IReadOnlyList<SyncCommissionRuleDto> SalesCommissionRules { get; init; } = Array.Empty<SyncCommissionRuleDto>();
    public IReadOnlyList<SyncProductKitDto> SalesProductKits { get; init; } = Array.Empty<SyncProductKitDto>();
    public IReadOnlyList<SyncServicePackageDto> SalesServicePackages { get; init; } = Array.Empty<SyncServicePackageDto>();
    public IReadOnlyList<SyncPrepaidBalanceDto> SalesPrepaidBalances { get; init; } = Array.Empty<SyncPrepaidBalanceDto>();
    public DateTimeOffset MaxUpdatedAt { get; init; }
    public bool HasMore { get; init; }
}
