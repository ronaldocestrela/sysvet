namespace Core.Application.Sync;

/// <summary>
/// Tutor row returned by sync pull (includes soft-deleted tombstones).
/// </summary>
public sealed class SyncTutorDto
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

/// <summary>
/// Pet row returned by sync pull (includes soft-deleted tombstones).
/// </summary>
public sealed class SyncPetDto
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

/// <summary>
/// Paginated pull page for client upsert.
/// </summary>
/// <summary>Appointment row returned by sync pull.</summary>
public sealed class SyncAppointmentDto
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

/// <summary>Schedule slot row returned by sync pull.</summary>
public sealed class SyncScheduleSlotDto
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

/// <summary>Evolution note embedded in medical record sync payload.</summary>
public sealed class SyncEvolutionNoteDto
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
}

/// <summary>Medical record row returned by sync pull.</summary>
public sealed class SyncMedicalRecordDto
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
    public IReadOnlyList<SyncEvolutionNoteDto> EvolutionNotes { get; init; } = Array.Empty<SyncEvolutionNoteDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Prescription template row returned by sync pull.</summary>
public sealed class SyncPrescriptionTemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<SyncPrescriptionLineDto> Items { get; init; } = Array.Empty<SyncPrescriptionLineDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Medication line in sync payloads.</summary>
public sealed class SyncPrescriptionLineDto
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

/// <summary>Issued prescription row returned by sync pull.</summary>
public sealed class SyncIssuedPrescriptionDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid? TemplateId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<SyncPrescriptionLineDto> Items { get; init; } = Array.Empty<SyncPrescriptionLineDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Clinical exam row returned by sync pull.</summary>
public sealed class SyncClinicalExamDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ResultSummary { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Clinical attachment metadata returned by sync pull (no bytes).</summary>
public sealed class SyncClinicalAttachmentDto
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
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Vaccine protocol returned by sync pull.</summary>
public sealed class SyncVaccineProtocolDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<SyncVaccineProtocolDoseDto> Doses { get; init; } = Array.Empty<SyncVaccineProtocolDoseDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Protocol dose line in sync pull.</summary>
public sealed class SyncVaccineProtocolDoseDto
{
    public Guid Id { get; init; }
    public int Sequence { get; init; }
    public string Label { get; init; } = string.Empty;
    public int MinAgeInDays { get; init; }
    public int? MaxAgeInDays { get; init; }
    public int? IntervalFromPreviousInDays { get; init; }
    public int? NextDoseIntervalInDays { get; init; }
}

/// <summary>Clinical quote line in sync payloads.</summary>
public sealed class SyncClinicalQuoteItemDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Kind { get; init; } = string.Empty;
    public Guid? ProductId { get; init; }
    public int SortOrder { get; init; }
}

/// <summary>Clinical quote returned by sync pull.</summary>
public sealed class SyncClinicalQuoteDto
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
    public IReadOnlyList<SyncClinicalQuoteItemDto> Items { get; init; } = Array.Empty<SyncClinicalQuoteItemDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Applied vaccine dose returned by sync pull.</summary>
public sealed class SyncVaccineDoseDto
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
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Bed line in ward sync pull.</summary>
public sealed class SyncBedDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>Ward unit returned by sync pull.</summary>
public sealed class SyncWardUnitDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<SyncBedDto> Beds { get; init; } = Array.Empty<SyncBedDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Medication order in hospitalization sync.</summary>
public sealed class SyncHospitalMedicationOrderDto
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

/// <summary>Administration slot in hospitalization sync.</summary>
public sealed class SyncMedicationAdministrationDto
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

/// <summary>Hospitalization returned by sync pull.</summary>
public sealed class SyncHospitalizationDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid BedId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset AdmittedAt { get; init; }
    public DateTimeOffset? DischargedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<SyncHospitalMedicationOrderDto> MedicationOrders { get; init; } = Array.Empty<SyncHospitalMedicationOrderDto>();
    public IReadOnlyList<SyncMedicationAdministrationDto> Administrations { get; init; } = Array.Empty<SyncMedicationAdministrationDto>();
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Inventory product row for sync pull.</summary>
public sealed class SyncInventoryProductDto
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
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Inventory product lot row for sync pull.</summary>
public sealed class SyncInventoryProductLotDto
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
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Inventory stock movement row for sync pull.</summary>
public sealed class SyncInventoryStockMovementDto
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
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Inventory supplier row for sync pull.</summary>
public sealed class SyncInventorySupplierDto
{
    public Guid Id { get; init; }
    public string LegalName { get; init; } = string.Empty;
    public string TradeName { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Cash register session row for sync pull.</summary>
public sealed class SyncSalesCashRegisterDto
{
    public Guid Id { get; init; }
    public Guid OpenedByUserId { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal ClosingBalance { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>POS order row for sync pull.</summary>
public sealed class SyncSalesOrderDto
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
    public string RowVersion { get; init; } = string.Empty;
    public IReadOnlyList<SyncSalesOrderItemDto> Items { get; init; } = Array.Empty<SyncSalesOrderItemDto>();
    public IReadOnlyList<SyncSalesOrderPaymentDto> Payments { get; init; } = Array.Empty<SyncSalesOrderPaymentDto>();
    public IReadOnlyList<SyncSalesCommissionAccrualDto> Commissions { get; init; } = Array.Empty<SyncSalesCommissionAccrualDto>();
    public IReadOnlyList<SyncSalesReturnDto> Returns { get; init; } = Array.Empty<SyncSalesReturnDto>();
}

/// <summary>Order line on sync pull.</summary>
public sealed class SyncSalesOrderItemDto
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

/// <summary>Commission accrual on sync pull.</summary>
public sealed class SyncSalesCommissionAccrualDto
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

/// <summary>Customer return on sync pull.</summary>
public sealed class SyncSalesReturnDto
{
    public Guid Id { get; init; }
    public decimal RefundAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<SyncSalesReturnLineDto> Lines { get; init; } = Array.Empty<SyncSalesReturnLineDto>();
}

/// <summary>Return line on sync pull.</summary>
public sealed class SyncSalesReturnLineDto
{
    public Guid Id { get; init; }
    public Guid OrderItemId { get; init; }
    public decimal Quantity { get; init; }
}

/// <summary>Commission rule for sync pull.</summary>
public sealed class SyncCommissionRuleDto
{
    public Guid Id { get; init; }
    public string Role { get; init; } = string.Empty;
    public string AppliesTo { get; init; } = string.Empty;
    public decimal RatePercent { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Product kit offer for sync pull.</summary>
public sealed class SyncProductKitDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public IReadOnlyList<SyncProductKitComponentDto> Components { get; init; } = Array.Empty<SyncProductKitComponentDto>();
}

/// <summary>Kit component line for sync pull.</summary>
public sealed class SyncProductKitComponentDto
{
    public Guid ProductId { get; init; }
    public decimal QuantityPerKit { get; init; }
}

/// <summary>Prepaid service package offer for sync pull.</summary>
public sealed class SyncServicePackageDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ServiceCode { get; init; } = string.Empty;
    public int UsesPerUnit { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Prepaid balance wallet for sync pull.</summary>
public sealed class SyncPrepaidBalanceDto
{
    public Guid Id { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public string ServiceCode { get; init; } = string.Empty;
    public int RemainingUses { get; init; }
    public int PurchasedUses { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Payment line on sync pull.</summary>
public sealed class SyncSalesOrderPaymentDto
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
    public IReadOnlyList<SyncSalesOrderPaymentRefundDto> Refunds { get; init; } =
        Array.Empty<SyncSalesOrderPaymentRefundDto>();
}

/// <summary>Refund line on sync pull.</summary>
public sealed class SyncSalesOrderPaymentRefundDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string? RefundNsu { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>Grooming appointment row from sync pull.</summary>
public sealed class SyncGroomingAppointmentDto
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

/// <summary>Groomer schedule slot from sync pull.</summary>
public sealed class SyncGroomingSlotDto
{
    public Guid Id { get; init; }
    public Guid GroomerId { get; init; }
    public DateTimeOffset Date { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsAvailable { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Grooming digital record from sync pull.</summary>
public sealed class SyncGroomingRecordDto
{
    public Guid Id { get; init; }
    public Guid GroomingAppointmentId { get; init; }
    public Guid GroomerId { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public string CoatNotes { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<SyncGroomingRecordSupplyLineDto> SupplyLines { get; init; } = Array.Empty<SyncGroomingRecordSupplyLineDto>();
}

/// <summary>Supply line on a grooming record.</summary>
public sealed class SyncGroomingRecordSupplyLineDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
}

/// <summary>Grooming service catalog row from sync pull.</summary>
public sealed class SyncGroomingServiceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ServiceType { get; init; } = string.Empty;
    public int DurationInMinutes { get; init; }
    public string? PrepaidServiceCode { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<SyncGroomingServiceSupplyLineDto> DefaultSupplies { get; init; } = Array.Empty<SyncGroomingServiceSupplyLineDto>();
}

/// <summary>Default supply on a grooming service.</summary>
public sealed class SyncGroomingServiceSupplyLineDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
}

public sealed class SyncFinanceTitleDto
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
    public IReadOnlyList<SyncTitleAllocationDto> Allocations { get; init; } = Array.Empty<SyncTitleAllocationDto>();
}

public sealed class SyncTitleAllocationDto
{
    public Guid Id { get; init; }
    public Guid FinancialTitleId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset PaidAt { get; init; }
    public string Method { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SyncFinanceCategoryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SyncFinanceCostCenterDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class PullChangesResult
{
    public IReadOnlyList<SyncTutorDto> Tutors { get; init; } = Array.Empty<SyncTutorDto>();
    public IReadOnlyList<SyncPetDto> Pets { get; init; } = Array.Empty<SyncPetDto>();
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
    public IReadOnlyList<SyncGroomingAppointmentDto> GroomingAppointments { get; init; } = Array.Empty<SyncGroomingAppointmentDto>();
    public IReadOnlyList<SyncGroomingSlotDto> GroomingSlots { get; init; } = Array.Empty<SyncGroomingSlotDto>();
    public IReadOnlyList<SyncGroomingRecordDto> GroomingRecords { get; init; } = Array.Empty<SyncGroomingRecordDto>();
    public IReadOnlyList<SyncGroomingServiceDto> GroomingServices { get; init; } = Array.Empty<SyncGroomingServiceDto>();
    public IReadOnlyList<SyncFinanceTitleDto> FinanceTitles { get; init; } = Array.Empty<SyncFinanceTitleDto>();
    public IReadOnlyList<SyncFinanceCategoryDto> FinanceCategories { get; init; } = Array.Empty<SyncFinanceCategoryDto>();
    public IReadOnlyList<SyncFinanceCostCenterDto> FinanceCostCenters { get; init; } = Array.Empty<SyncFinanceCostCenterDto>();
    public DateTimeOffset NextSince { get; init; }
    public bool HasMore { get; init; }
}
