using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineVaccines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Pets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VaccineDoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    NextDueDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ProtocolId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProtocolDoseId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccineDoses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaccineProtocols",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Species = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccineProtocols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaccineProtocolDoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VaccineProtocolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MinAgeInDays = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAgeInDays = table.Column<int>(type: "INTEGER", nullable: true),
                    IntervalFromPreviousInDays = table.Column<int>(type: "INTEGER", nullable: true),
                    NextDoseIntervalInDays = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccineProtocolDoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaccineProtocolDoses_VaccineProtocols_VaccineProtocolId",
                        column: x => x.VaccineProtocolId,
                        principalTable: "VaccineProtocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VaccineDoses_NextDueDate",
                table: "VaccineDoses",
                column: "NextDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineDoses_PetId",
                table: "VaccineDoses",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineProtocolDoses_VaccineProtocolId",
                table: "VaccineProtocolDoses",
                column: "VaccineProtocolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaccineDoses");

            migrationBuilder.DropTable(
                name: "VaccineProtocolDoses");

            migrationBuilder.DropTable(
                name: "VaccineProtocols");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Pets");
        }
    }
}
