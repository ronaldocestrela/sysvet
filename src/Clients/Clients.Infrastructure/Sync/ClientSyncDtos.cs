namespace Clients.Infrastructure.Sync;

/// <summary>
/// Client-side mirror of API sync HTTP contracts.
/// </summary>
public sealed class ClientSyncPushResult
{
    public IReadOnlyList<Guid> ProcessedIds { get; init; } = Array.Empty<Guid>();
    public Guid? FailedMessageId { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsPermanentFailure { get; init; }
}

/// <summary>
/// Pull page returned by the sync API.
/// </summary>
public sealed class ClientPullChangesResult
{
    public IReadOnlyList<ClientSyncTutorDto> Tutors { get; init; } = Array.Empty<ClientSyncTutorDto>();
    public IReadOnlyList<ClientSyncPetDto> Pets { get; init; } = Array.Empty<ClientSyncPetDto>();
    public IReadOnlyList<ClientSyncAppointmentDto> Appointments { get; init; } = Array.Empty<ClientSyncAppointmentDto>();
    public IReadOnlyList<ClientSyncScheduleSlotDto> ScheduleSlots { get; init; } = Array.Empty<ClientSyncScheduleSlotDto>();
    public IReadOnlyList<ClientSyncMedicalRecordDto> MedicalRecords { get; init; } = Array.Empty<ClientSyncMedicalRecordDto>();
    public IReadOnlyList<ClientSyncPrescriptionTemplateDto> PrescriptionTemplates { get; init; } = Array.Empty<ClientSyncPrescriptionTemplateDto>();
    public IReadOnlyList<ClientSyncIssuedPrescriptionDto> IssuedPrescriptions { get; init; } = Array.Empty<ClientSyncIssuedPrescriptionDto>();
    public IReadOnlyList<ClientSyncClinicalExamDto> ClinicalExams { get; init; } = Array.Empty<ClientSyncClinicalExamDto>();
    public IReadOnlyList<ClientSyncClinicalAttachmentDto> ClinicalAttachments { get; init; } = Array.Empty<ClientSyncClinicalAttachmentDto>();
    public IReadOnlyList<ClientSyncVaccineProtocolDto> VaccineProtocols { get; init; } = Array.Empty<ClientSyncVaccineProtocolDto>();
    public IReadOnlyList<ClientSyncVaccineDoseDto> VaccineDoses { get; init; } = Array.Empty<ClientSyncVaccineDoseDto>();
    public IReadOnlyList<ClientSyncClinicalQuoteDto> ClinicalQuotes { get; init; } = Array.Empty<ClientSyncClinicalQuoteDto>();
    public IReadOnlyList<ClientSyncWardUnitDto> WardUnits { get; init; } = Array.Empty<ClientSyncWardUnitDto>();
    public IReadOnlyList<ClientSyncHospitalizationDto> Hospitalizations { get; init; } = Array.Empty<ClientSyncHospitalizationDto>();
    public IReadOnlyList<ClientSyncInventoryProductDto> InventoryProducts { get; init; } = Array.Empty<ClientSyncInventoryProductDto>();
    public IReadOnlyList<ClientSyncInventoryProductLotDto> InventoryProductLots { get; init; } = Array.Empty<ClientSyncInventoryProductLotDto>();
    public IReadOnlyList<ClientSyncInventorySupplierDto> InventorySuppliers { get; init; } = Array.Empty<ClientSyncInventorySupplierDto>();
    public IReadOnlyList<ClientSyncInventoryStockMovementDto> InventoryStockMovements { get; init; } = Array.Empty<ClientSyncInventoryStockMovementDto>();
    public IReadOnlyList<ClientSyncSalesCashRegisterDto> SalesCashRegisters { get; init; } = Array.Empty<ClientSyncSalesCashRegisterDto>();
    public IReadOnlyList<ClientSyncSalesOrderDto> SalesOrders { get; init; } = Array.Empty<ClientSyncSalesOrderDto>();
    public IReadOnlyList<ClientSyncCommissionRuleDto> SalesCommissionRules { get; init; } = Array.Empty<ClientSyncCommissionRuleDto>();
    public IReadOnlyList<ClientSyncProductKitDto> SalesProductKits { get; init; } = Array.Empty<ClientSyncProductKitDto>();
    public IReadOnlyList<ClientSyncServicePackageDto> SalesServicePackages { get; init; } = Array.Empty<ClientSyncServicePackageDto>();
    public IReadOnlyList<ClientSyncPrepaidBalanceDto> SalesPrepaidBalances { get; init; } = Array.Empty<ClientSyncPrepaidBalanceDto>();
    public IReadOnlyList<ClientSyncGroomingAppointmentDto> GroomingAppointments { get; init; } = Array.Empty<ClientSyncGroomingAppointmentDto>();
    public IReadOnlyList<ClientSyncGroomingSlotDto> GroomingSlots { get; init; } = Array.Empty<ClientSyncGroomingSlotDto>();
    public IReadOnlyList<ClientSyncGroomingRecordDto> GroomingRecords { get; init; } = Array.Empty<ClientSyncGroomingRecordDto>();
    public IReadOnlyList<ClientSyncGroomingServiceDto> GroomingServices { get; init; } = Array.Empty<ClientSyncGroomingServiceDto>();
    public IReadOnlyList<ClientSyncFinanceTitleDto> FinanceTitles { get; init; } = Array.Empty<ClientSyncFinanceTitleDto>();
    public IReadOnlyList<ClientSyncFinanceCategoryDto> FinanceCategories { get; init; } = Array.Empty<ClientSyncFinanceCategoryDto>();
    public IReadOnlyList<ClientSyncFinanceCostCenterDto> FinanceCostCenters { get; init; } = Array.Empty<ClientSyncFinanceCostCenterDto>();
    public DateTimeOffset NextSince { get; init; }
    public bool HasMore { get; init; }
}

public sealed class ClientSyncFinanceTitleDto
{
    public Guid Id { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public Guid? SourceId { get; init; }
    public string SourceInstallmentKey { get; init; } = string.Empty;
    public string PartyKind { get; init; } = string.Empty;
    public Guid? PartyId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? CostCenterId { get; init; }
    public DateOnly IssueDate { get; init; }
    public DateOnly DueDate { get; init; }
    public decimal OriginalAmount { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<ClientSyncTitleAllocationDto> Allocations { get; init; } = Array.Empty<ClientSyncTitleAllocationDto>();
}

public sealed class ClientSyncTitleAllocationDto
{
    public Guid Id { get; init; }
    public Guid FinancialTitleId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset PaidAt { get; init; }
    public string Method { get; init; } = string.Empty;
    public string? ExternalReference { get; init; }
    public Guid CorrelationId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncFinanceCategoryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncFinanceCostCenterDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncGroomingAppointmentDto
{
    public Guid Id { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public Guid GroomerId { get; init; }
    public Guid GroomingServiceId { get; init; }
    public DateTimeOffset Date { get; init; }
    public int DurationInMinutes { get; init; }
    public string Notes { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

public sealed class ClientSyncGroomingSlotDto
{
    public Guid Id { get; init; }
    public Guid GroomerId { get; init; }
    public DateTimeOffset Date { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsAvailable { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncGroomingRecordDto
{
    public Guid Id { get; init; }
    public Guid GroomingAppointmentId { get; init; }
    public Guid GroomerId { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public string CoatNotes { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<ClientSyncGroomingRecordSupplyLineDto> SupplyLines { get; init; } = Array.Empty<ClientSyncGroomingRecordSupplyLineDto>();
}

public sealed class ClientSyncGroomingRecordSupplyLineDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
}

public sealed class ClientSyncGroomingServiceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ServiceType { get; init; } = string.Empty;
    public int DurationInMinutes { get; init; }
    public string? PrepaidServiceCode { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<ClientSyncGroomingServiceSupplyLineDto> DefaultSupplies { get; init; } = Array.Empty<ClientSyncGroomingServiceSupplyLineDto>();
}

public sealed class ClientSyncGroomingServiceSupplyLineDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
}

/// <summary>Tutor row from sync pull.</summary>
public sealed class ClientSyncTutorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Pet row from sync pull.</summary>
public sealed class ClientSyncPetDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public string Breed { get; init; } = string.Empty;
    public string Sex { get; init; } = string.Empty;
    public Guid TutorId { get; init; }
    public DateOnly? BirthDate { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Appointment row from sync pull.</summary>
public sealed class ClientSyncAppointmentDto
{
    public Guid Id { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public DateTimeOffset Date { get; init; }
    public int DurationInMinutes { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Evolution note in medical record sync payload.</summary>
public sealed class ClientSyncEvolutionNoteDto
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
}

/// <summary>Medical record row from sync pull.</summary>
public sealed class ClientSyncMedicalRecordDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public string Anamnesis { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string Prescription { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal? VitalWeightKg { get; init; }
    public decimal? VitalTemperatureC { get; init; }
    public int? VitalHeartRateBpm { get; init; }
    public int? VitalRespiratoryRateBpm { get; init; }
    public string? VitalMucousMembranes { get; init; }
    public string? VitalCapillaryRefillTime { get; init; }
    public DateTimeOffset? VitalMeasuredAt { get; init; }
    public IReadOnlyList<ClientSyncEvolutionNoteDto> EvolutionNotes { get; init; } = Array.Empty<ClientSyncEvolutionNoteDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Schedule slot row from sync pull.</summary>
public sealed class ClientSyncPrescriptionLineDto
{
    public Guid Id { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Concentration { get; init; } = string.Empty;
    public string Dose { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}

public sealed class ClientSyncPrescriptionTemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<ClientSyncPrescriptionLineDto> Items { get; init; } = Array.Empty<ClientSyncPrescriptionLineDto>();
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncIssuedPrescriptionDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid? TemplateId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<ClientSyncPrescriptionLineDto> Items { get; init; } = Array.Empty<ClientSyncPrescriptionLineDto>();
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncClinicalExamDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ResultSummary { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncClinicalAttachmentDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid? MedicalRecordId { get; init; }
    public Guid? ClinicalExamId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string BlobKey { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncScheduleSlotDto
{
    public Guid Id { get; init; }
    public Guid VeterinarianId { get; init; }
    public DateTimeOffset Date { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsAvailable { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

public sealed class ClientSyncVaccineProtocolDoseDto
{
    public Guid Id { get; init; }
    public int Sequence { get; init; }
    public string Label { get; init; } = string.Empty;
    public int MinAgeInDays { get; init; }
    public int? MaxAgeInDays { get; init; }
    public int? IntervalFromPreviousInDays { get; init; }
    public int? NextDoseIntervalInDays { get; init; }
}

public sealed class ClientSyncVaccineProtocolDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<ClientSyncVaccineProtocolDoseDto> Doses { get; init; } = Array.Empty<ClientSyncVaccineProtocolDoseDto>();
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncVaccineDoseDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? NextDueDate { get; init; }
    public Guid? ProtocolId { get; init; }
    public Guid? ProtocolDoseId { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncClinicalQuoteItemDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Kind { get; init; } = string.Empty;
    public Guid? ProductId { get; init; }
    public int SortOrder { get; init; }
}

public sealed class ClientSyncClinicalQuoteDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public Guid TutorId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ConversionStatus { get; init; } = string.Empty;
    public Guid? ConvertedOrderId { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTimeOffset? SentAt { get; init; }
    public DateTimeOffset? DecidedAt { get; init; }
    public IReadOnlyList<ClientSyncClinicalQuoteItemDto> Items { get; init; } = Array.Empty<ClientSyncClinicalQuoteItemDto>();
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncBedDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public sealed class ClientSyncWardUnitDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<ClientSyncBedDto> Beds { get; init; } = Array.Empty<ClientSyncBedDto>();
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncHospitalMedicationOrderDto
{
    public Guid Id { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Dose { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string DailyTimesCsv { get; init; } = string.Empty;
    public DateOnly StartsOn { get; init; }
    public DateOnly EndsOn { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncMedicationAdministrationDto
{
    public Guid Id { get; init; }
    public Guid MedicationOrderId { get; init; }
    public DateTimeOffset ScheduledAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? ActorId { get; init; }
    public DateTimeOffset? ActedAt { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncInventoryProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal ReorderLevel { get; init; }
    public decimal TargetStock { get; init; }
    public string Category { get; init; } = string.Empty;
    public Guid? SupplierId { get; init; }
    public string Ncm { get; init; } = string.Empty;
    public string? Cest { get; init; }
    public int MerchandiseOrigin { get; init; }
    public decimal AverageCost { get; init; }
    public bool RequiresLot { get; init; }
    public decimal UnitsPerPackage { get; init; } = 1m;
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncInventoryProductLotDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public DateTimeOffset? ExpirationDate { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Quantity { get; init; }
    public bool IsFractional { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncInventoryStockMovementDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public Guid? ProductLotId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? AdjustmentDirection { get; init; }
    public decimal Quantity { get; init; }
    public string? BatchNumber { get; init; }
    public DateTimeOffset? ExpirationDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public Guid? SupplierId { get; init; }
    public DateTimeOffset Date { get; init; }
    public Guid? CorrelationId { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncInventorySupplierDto
{
    public Guid Id { get; init; }
    public string LegalName { get; init; } = string.Empty;
    public string TradeName { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncHospitalizationDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid BedId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset AdmittedAt { get; init; }
    public DateTimeOffset? DischargedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<ClientSyncHospitalMedicationOrderDto> MedicationOrders { get; init; } = Array.Empty<ClientSyncHospitalMedicationOrderDto>();
    public IReadOnlyList<ClientSyncMedicationAdministrationDto> Administrations { get; init; } = Array.Empty<ClientSyncMedicationAdministrationDto>();
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncSalesCashRegisterDto
{
    public Guid Id { get; init; }
    public Guid OpenedByUserId { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal ExpectedClosingBalance { get; init; }
    public decimal ClosingBalance { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<ClientSyncSalesCashMovementDto> Movements { get; init; } = Array.Empty<ClientSyncSalesCashMovementDto>();
}

public sealed class ClientSyncSalesCashMovementDto
{
    public Guid Id { get; init; }
    public Guid CashRegisterId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncSalesOrderDto
{
    public Guid Id { get; init; }
    public Guid CashRegisterId { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? TutorId { get; init; }
    public Guid? PetId { get; init; }
    public Guid? SourceQuoteId { get; init; }
    public Guid SellerUserId { get; init; }
    public decimal DiscountPercent { get; init; }
    public string FinanceIntegrationStatus { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<ClientSyncSalesOrderItemDto> Items { get; init; } = Array.Empty<ClientSyncSalesOrderItemDto>();
    public IReadOnlyList<ClientSyncSalesOrderPaymentDto> Payments { get; init; } = Array.Empty<ClientSyncSalesOrderPaymentDto>();
    public IReadOnlyList<ClientSyncSalesCommissionAccrualDto> Commissions { get; init; } = Array.Empty<ClientSyncSalesCommissionAccrualDto>();
    public IReadOnlyList<ClientSyncSalesReturnDto> Returns { get; init; } = Array.Empty<ClientSyncSalesReturnDto>();
}

public sealed class ClientSyncSalesOrderItemDto
{
    public Guid Id { get; init; }
    public string Kind { get; init; } = string.Empty;
    public Guid? ProductId { get; init; }
    public Guid? CatalogOfferId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public Guid? PerformerUserId { get; init; }
    public string? PerformerRole { get; init; }
    public decimal ReturnedQuantity { get; init; }
}

public sealed class ClientSyncSalesCommissionAccrualDto
{
    public Guid Id { get; init; }
    public Guid OrderItemId { get; init; }
    public Guid PayeeUserId { get; init; }
    public string Role { get; init; } = string.Empty;
    public decimal RatePercent { get; init; }
    public decimal BaseAmount { get; init; }
    public decimal CommissionAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class ClientSyncSalesReturnDto
{
    public Guid Id { get; init; }
    public decimal RefundAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<ClientSyncSalesReturnLineDto> Lines { get; init; } = Array.Empty<ClientSyncSalesReturnLineDto>();
}

public sealed class ClientSyncSalesReturnLineDto
{
    public Guid Id { get; init; }
    public Guid OrderItemId { get; init; }
    public decimal Quantity { get; init; }
}

public sealed class ClientSyncCommissionRuleDto
{
    public Guid Id { get; init; }
    public string Role { get; init; } = string.Empty;
    public string AppliesTo { get; init; } = string.Empty;
    public decimal RatePercent { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncProductKitDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<ClientSyncProductKitComponentDto> Components { get; init; } = Array.Empty<ClientSyncProductKitComponentDto>();
}

public sealed class ClientSyncProductKitComponentDto
{
    public Guid ProductId { get; init; }
    public decimal QuantityPerKit { get; init; }
}

public sealed class ClientSyncServicePackageDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ServiceCode { get; init; } = string.Empty;
    public int UsesPerUnit { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncPrepaidBalanceDto
{
    public Guid Id { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public string ServiceCode { get; init; } = string.Empty;
    public int RemainingUses { get; init; }
    public int PurchasedUses { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ClientSyncSalesOrderPaymentDto
{
    public Guid Id { get; init; }
    public string Method { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Nsu { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? Provider { get; init; }
    public string? TerminalId { get; init; }
    public string? Brand { get; init; }
    public int Installments { get; init; } = 1;
    public IReadOnlyList<ClientSyncSalesOrderPaymentRefundDto> Refunds { get; init; } =
        Array.Empty<ClientSyncSalesOrderPaymentRefundDto>();
}

public sealed class ClientSyncSalesOrderPaymentRefundDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string? RefundNsu { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
