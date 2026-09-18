using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalExamsPrescriptionsAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrescriptionTemplates",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Species = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_PrescriptionTemplates", x => x.Id));

            migrationBuilder.CreateTable(
                name: "PrescriptionTemplateItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrescriptionTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Concentration = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Dose = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Route = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionTemplateItems_PrescriptionTemplates_PrescriptionTemplateId",
                        column: x => x.PrescriptionTemplateId,
                        principalSchema: "dbo",
                        principalTable: "PrescriptionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssuedPrescriptions",
                schema: "dbo",
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
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssuedPrescriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Concentration = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Dose = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Route = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
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
                        principalSchema: "dbo",
                        principalTable: "IssuedPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalExams",
                schema: "dbo",
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
                name: "ClinicalAttachments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClinicalExamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    BlobKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ClinicalAttachments", x => x.Id));

            migrationBuilder.CreateIndex(name: "IX_PrescriptionTemplateItems_PrescriptionTemplateId", schema: "dbo", table: "PrescriptionTemplateItems", column: "PrescriptionTemplateId");
            migrationBuilder.CreateIndex(name: "IX_IssuedPrescriptions_AppointmentId", schema: "dbo", table: "IssuedPrescriptions", column: "AppointmentId");
            migrationBuilder.CreateIndex(name: "IX_IssuedPrescriptions_PetId", schema: "dbo", table: "IssuedPrescriptions", column: "PetId");
            migrationBuilder.CreateIndex(name: "IX_PrescriptionItems_IssuedPrescriptionId", schema: "dbo", table: "PrescriptionItems", column: "IssuedPrescriptionId");
            migrationBuilder.CreateIndex(name: "IX_ClinicalExams_AppointmentId", schema: "dbo", table: "ClinicalExams", column: "AppointmentId");
            migrationBuilder.CreateIndex(name: "IX_ClinicalExams_PetId", schema: "dbo", table: "ClinicalExams", column: "PetId");
            migrationBuilder.CreateIndex(name: "IX_ClinicalAttachments_AppointmentId", schema: "dbo", table: "ClinicalAttachments", column: "AppointmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClinicalAttachments", schema: "dbo");
            migrationBuilder.DropTable(name: "ClinicalExams", schema: "dbo");
            migrationBuilder.DropTable(name: "PrescriptionItems", schema: "dbo");
            migrationBuilder.DropTable(name: "PrescriptionTemplateItems", schema: "dbo");
            migrationBuilder.DropTable(name: "IssuedPrescriptions", schema: "dbo");
            migrationBuilder.DropTable(name: "PrescriptionTemplates", schema: "dbo");
        }
    }
}
