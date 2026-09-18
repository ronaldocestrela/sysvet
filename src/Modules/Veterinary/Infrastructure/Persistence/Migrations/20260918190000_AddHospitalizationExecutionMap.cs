using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalizationExecutionMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrescriptionExecutions",
                schema: "dbo");

            migrationBuilder.AddColumn<Guid>(
                name: "BedId",
                schema: "dbo",
                table: "Hospitalizations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.CreateTable(
                name: "WardUnits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_WardUnits", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Beds",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WardUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beds_WardUnits_WardUnitId",
                        column: x => x.WardUnitId,
                        principalSchema: "dbo",
                        principalTable: "WardUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HospitalMedicationOrders",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HospitalizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Dose = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Route = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DailyTimesCsv = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalMedicationOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HospitalMedicationOrders_Hospitalizations_HospitalizationId",
                        column: x => x.HospitalizationId,
                        principalSchema: "dbo",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationAdministrations",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HospitalizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationOrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationAdministrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationAdministrations_Hospitalizations_HospitalizationId",
                        column: x => x.HospitalizationId,
                        principalSchema: "dbo",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationProgressNotes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HospitalizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationProgressNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HospitalizationProgressNotes_Hospitalizations_HospitalizationId",
                        column: x => x.HospitalizationId,
                        principalSchema: "dbo",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HospitalProcedures",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HospitalizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    VeterinarianId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PerformedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HospitalProcedures_Hospitalizations_HospitalizationId",
                        column: x => x.HospitalizationId,
                        principalSchema: "dbo",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Beds_WardUnitId", schema: "dbo", table: "Beds", column: "WardUnitId");
            migrationBuilder.CreateIndex(name: "IX_Hospitalizations_BedId", schema: "dbo", table: "Hospitalizations", column: "BedId", unique: true, filter: "[Status] = 1");
            migrationBuilder.CreateIndex(name: "IX_Hospitalizations_PetId", schema: "dbo", table: "Hospitalizations", column: "PetId", unique: true, filter: "[Status] = 1");
            migrationBuilder.CreateIndex(name: "IX_HospitalMedicationOrders_HospitalizationId", schema: "dbo", table: "HospitalMedicationOrders", column: "HospitalizationId");
            migrationBuilder.CreateIndex(name: "IX_MedicationAdministrations_HospitalizationId", schema: "dbo", table: "MedicationAdministrations", column: "HospitalizationId");
            migrationBuilder.CreateIndex(name: "IX_MedicationAdministrations_MedicationOrderId", schema: "dbo", table: "MedicationAdministrations", column: "MedicationOrderId");
            migrationBuilder.CreateIndex(name: "IX_MedicationAdministrations_HospitalizationId_ScheduledAt", schema: "dbo", table: "MedicationAdministrations", columns: new[] { "HospitalizationId", "ScheduledAt" });
            migrationBuilder.CreateIndex(name: "IX_HospitalizationProgressNotes_HospitalizationId", schema: "dbo", table: "HospitalizationProgressNotes", column: "HospitalizationId");
            migrationBuilder.CreateIndex(name: "IX_HospitalProcedures_HospitalizationId", schema: "dbo", table: "HospitalProcedures", column: "HospitalizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Beds", schema: "dbo");
            migrationBuilder.DropTable(name: "HospitalMedicationOrders", schema: "dbo");
            migrationBuilder.DropTable(name: "MedicationAdministrations", schema: "dbo");
            migrationBuilder.DropTable(name: "HospitalizationProgressNotes", schema: "dbo");
            migrationBuilder.DropTable(name: "HospitalProcedures", schema: "dbo");
            migrationBuilder.DropTable(name: "WardUnits", schema: "dbo");
            migrationBuilder.DropColumn(name: "BedId", schema: "dbo", table: "Hospitalizations");

            migrationBuilder.CreateTable(
                name: "PrescriptionExecutions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Dose = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ExecutedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    HospitalizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionExecutions_Hospitalizations_HospitalizationId",
                        column: x => x.HospitalizationId,
                        principalSchema: "dbo",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
