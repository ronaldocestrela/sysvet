using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOfflineMedicalRecords : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MedicalRecords",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                VeterinarianId = table.Column<Guid>(type: "TEXT", nullable: false),
                TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                Anamnesis = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                Diagnosis = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                Prescription = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                VitalWeightKg = table.Column<decimal>(type: "TEXT", nullable: true),
                VitalTemperatureC = table.Column<decimal>(type: "TEXT", nullable: true),
                VitalHeartRateBpm = table.Column<int>(type: "INTEGER", nullable: true),
                VitalRespiratoryRateBpm = table.Column<int>(type: "INTEGER", nullable: true),
                VitalMucousMembranes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                VitalCapillaryRefillTime = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                VitalMeasuredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_MedicalRecords", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_MedicalRecords_AppointmentId", table: "MedicalRecords", column: "AppointmentId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_MedicalRecords_PetId", table: "MedicalRecords", column: "PetId");

        migrationBuilder.CreateTable(
            name: "EvolutionNotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                MedicalRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                AuthorId = table.Column<Guid>(type: "TEXT", nullable: false),
                Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                RecordedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EvolutionNotes", x => x.Id);
                table.ForeignKey(
                    name: "FK_EvolutionNotes_MedicalRecords_MedicalRecordId",
                    column: x => x.MedicalRecordId,
                    principalTable: "MedicalRecords",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_EvolutionNotes_MedicalRecordId", table: "EvolutionNotes", column: "MedicalRecordId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EvolutionNotes");
        migrationBuilder.DropTable(name: "MedicalRecords");
    }
}
