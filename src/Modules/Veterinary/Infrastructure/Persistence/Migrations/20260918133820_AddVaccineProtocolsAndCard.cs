using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccineProtocolsAndCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProtocolDoseId",
                schema: "dbo",
                table: "VaccineDoses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProtocolId",
                schema: "dbo",
                table: "VaccineDoses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VaccineProtocols",
                schema: "dbo",
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
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "VaccineProtocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VaccineDoses_NextDueDate",
                schema: "dbo",
                table: "VaccineDoses",
                column: "NextDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineDoses_PetId",
                schema: "dbo",
                table: "VaccineDoses",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineProtocolDoses_VaccineProtocolId",
                schema: "dbo",
                table: "VaccineProtocolDoses",
                column: "VaccineProtocolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaccineProtocolDoses",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VaccineProtocols",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_VaccineDoses_NextDueDate",
                schema: "dbo",
                table: "VaccineDoses");

            migrationBuilder.DropIndex(
                name: "IX_VaccineDoses_PetId",
                schema: "dbo",
                table: "VaccineDoses");

            migrationBuilder.DropColumn(
                name: "ProtocolDoseId",
                schema: "dbo",
                table: "VaccineDoses");

            migrationBuilder.DropColumn(
                name: "ProtocolId",
                schema: "dbo",
                table: "VaccineDoses");
        }
    }
}
