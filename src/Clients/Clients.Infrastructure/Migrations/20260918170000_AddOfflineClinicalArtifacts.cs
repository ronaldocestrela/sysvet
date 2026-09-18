using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOfflineClinicalArtifacts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClinicalExams",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Category = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                ResultSummary = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ClinicalExams", x => x.Id));

        migrationBuilder.CreateTable(
            name: "IssuedPrescriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                VeterinarianId = table.Column<Guid>(type: "TEXT", nullable: false),
                TemplateId = table.Column<Guid>(type: "TEXT", nullable: true),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_IssuedPrescriptions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PrescriptionItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                IssuedPrescriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                MedicationName = table.Column<string>(type: "TEXT", nullable: false),
                Concentration = table.Column<string>(type: "TEXT", nullable: false),
                Dose = table.Column<string>(type: "TEXT", nullable: false),
                Route = table.Column<string>(type: "TEXT", nullable: false),
                Frequency = table.Column<string>(type: "TEXT", nullable: false),
                Duration = table.Column<string>(type: "TEXT", nullable: false),
                Instructions = table.Column<string>(type: "TEXT", nullable: false),
                SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_PrescriptionItems_IssuedPrescriptions_IssuedPrescriptionId",
                    column: x => x.IssuedPrescriptionId,
                    principalTable: "IssuedPrescriptions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ClinicalAttachments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                MedicalRecordId = table.Column<Guid>(type: "TEXT", nullable: true),
                ClinicalExamId = table.Column<Guid>(type: "TEXT", nullable: true),
                FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                ContentType = table.Column<string>(type: "TEXT", nullable: false),
                SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                Kind = table.Column<int>(type: "INTEGER", nullable: false),
                BlobKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ClinicalAttachments", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_ClinicalExams_AppointmentId", table: "ClinicalExams", column: "AppointmentId");
        migrationBuilder.CreateIndex(name: "IX_IssuedPrescriptions_AppointmentId", table: "IssuedPrescriptions", column: "AppointmentId");
        migrationBuilder.CreateIndex(name: "IX_PrescriptionItems_IssuedPrescriptionId", table: "PrescriptionItems", column: "IssuedPrescriptionId");
        migrationBuilder.CreateIndex(name: "IX_ClinicalAttachments_AppointmentId", table: "ClinicalAttachments", column: "AppointmentId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ClinicalAttachments");
        migrationBuilder.DropTable(name: "PrescriptionItems");
        migrationBuilder.DropTable(name: "ClinicalExams");
        migrationBuilder.DropTable(name: "IssuedPrescriptions");
    }
}
