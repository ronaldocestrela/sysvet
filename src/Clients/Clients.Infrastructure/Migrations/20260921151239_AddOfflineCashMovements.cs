using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineCashMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                table: "TitleAllocations",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedClosingBalance",
                table: "SalesCashRegisters",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedClosingCurrency",
                table: "SalesCashRegisters",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SalesCashMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CashRegisterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountCurrency = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCashMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesCashMovements_SalesCashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "SalesCashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesCashMovements_CashRegisterId",
                table: "SalesCashMovements",
                column: "CashRegisterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesCashMovements");

            migrationBuilder.DropColumn(
                name: "ExternalReference",
                table: "TitleAllocations");

            migrationBuilder.DropColumn(
                name: "ExpectedClosingBalance",
                table: "SalesCashRegisters");

            migrationBuilder.DropColumn(
                name: "ExpectedClosingCurrency",
                table: "SalesCashRegisters");
        }
    }
}
