using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Petshop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPetshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "GroomingAppointments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroomingServiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomingAppointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroomingRecords",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroomingAppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CoatNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomingRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroomingServices",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ServiceType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DurationInMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    PrepaidServiceCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomingServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroomingSlots",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomingSlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroomingRecordSupplyLines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroomingRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomingRecordSupplyLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomingRecordSupplyLines_GroomingRecords_GroomingRecordId",
                        column: x => x.GroomingRecordId,
                        principalSchema: "dbo",
                        principalTable: "GroomingRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroomingServiceSupplyLines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroomingServiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomingServiceSupplyLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomingServiceSupplyLines_GroomingServices_GroomingServiceId",
                        column: x => x.GroomingServiceId,
                        principalSchema: "dbo",
                        principalTable: "GroomingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecords_GroomingAppointmentId",
                schema: "dbo",
                table: "GroomingRecords",
                column: "GroomingAppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecords_PetId",
                schema: "dbo",
                table: "GroomingRecords",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomingRecordSupplyLines_GroomingRecordId",
                schema: "dbo",
                table: "GroomingRecordSupplyLines",
                column: "GroomingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomingServiceSupplyLines_GroomingServiceId",
                schema: "dbo",
                table: "GroomingServiceSupplyLines",
                column: "GroomingServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroomingAppointments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GroomingRecordSupplyLines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GroomingServiceSupplyLines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GroomingSlots",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GroomingRecords",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GroomingServices",
                schema: "dbo");
        }
    }
}
