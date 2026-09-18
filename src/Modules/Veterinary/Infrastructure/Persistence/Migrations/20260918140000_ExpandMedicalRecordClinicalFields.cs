using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandMedicalRecordClinicalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Anamnesis",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "VitalWeightKg",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VitalTemperatureC",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VitalHeartRateBpm",
                schema: "dbo",
                table: "MedicalRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VitalRespiratoryRateBpm",
                schema: "dbo",
                table: "MedicalRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitalMucousMembranes",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitalCapillaryRefillTime",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VitalMeasuredAt",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_AppointmentId",
                schema: "dbo",
                table: "MedicalRecords",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PetId",
                schema: "dbo",
                table: "MedicalRecords",
                column: "PetId");

            migrationBuilder.CreateTable(
                name: "EvolutionNotes",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "MedicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvolutionNotes_MedicalRecordId",
                schema: "dbo",
                table: "EvolutionNotes",
                column: "MedicalRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvolutionNotes",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_AppointmentId",
                schema: "dbo",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_PetId",
                schema: "dbo",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(name: "Anamnesis", schema: "dbo", table: "MedicalRecords");
            migrationBuilder.DropColumn(name: "VitalWeightKg", schema: "dbo", table: "MedicalRecords");
            migrationBuilder.DropColumn(name: "VitalTemperatureC", schema: "dbo", table: "MedicalRecords");
            migrationBuilder.DropColumn(name: "VitalHeartRateBpm", schema: "dbo", table: "MedicalRecords");
            migrationBuilder.DropColumn(name: "VitalRespiratoryRateBpm", schema: "dbo", table: "MedicalRecords");
            migrationBuilder.DropColumn(name: "VitalMucousMembranes", schema: "dbo", table: "MedicalRecords");
            migrationBuilder.DropColumn(name: "VitalCapillaryRefillTime", schema: "dbo", table: "MedicalRecords");
            migrationBuilder.DropColumn(name: "VitalMeasuredAt", schema: "dbo", table: "MedicalRecords");
        }
    }
}
