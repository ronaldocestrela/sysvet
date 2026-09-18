using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineHospitalizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hospitalizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VeterinarianId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BedId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AdmittedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DischargedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospitalizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WardUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WardUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationProgressNotes",
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
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HospitalMedicationOrders",
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
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HospitalProcedures",
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
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationAdministrations",
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
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Beds",
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
                        principalTable: "WardUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beds_WardUnitId",
                table: "Beds",
                column: "WardUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationProgressNotes_HospitalizationId",
                table: "HospitalizationProgressNotes",
                column: "HospitalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalMedicationOrders_HospitalizationId",
                table: "HospitalMedicationOrders",
                column: "HospitalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalProcedures_HospitalizationId",
                table: "HospitalProcedures",
                column: "HospitalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationAdministrations_HospitalizationId",
                table: "MedicationAdministrations",
                column: "HospitalizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beds");

            migrationBuilder.DropTable(
                name: "HospitalizationProgressNotes");

            migrationBuilder.DropTable(
                name: "HospitalMedicationOrders");

            migrationBuilder.DropTable(
                name: "HospitalProcedures");

            migrationBuilder.DropTable(
                name: "MedicationAdministrations");

            migrationBuilder.DropTable(
                name: "WardUnits");

            migrationBuilder.DropTable(
                name: "Hospitalizations");
        }
    }
}
