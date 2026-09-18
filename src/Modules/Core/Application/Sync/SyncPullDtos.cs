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
    public string Category { get; init; } = string.Empty;
    public Guid? SupplierId { get; init; }
    public string Ncm { get; init; } = string.Empty;
    public string? Cest { get; init; }
    public int MerchandiseOrigin { get; init; }
    public decimal AverageCost { get; init; }
    public bool RequiresLot { get; init; }
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
    public DateTimeOffset NextSince { get; init; }
    public bool HasMore { get; init; }
}
