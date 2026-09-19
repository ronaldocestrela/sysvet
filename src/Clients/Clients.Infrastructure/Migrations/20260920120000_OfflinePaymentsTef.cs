using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OfflinePaymentsTef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCode",
                table: "SalesPayments",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "SalesPayments",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Installments",
                table: "SalesPayments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Nsu",
                table: "SalesPayments",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "SalesPayments",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminalId",
                table: "SalesPayments",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesPaymentRefunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaymentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    RefundNsu = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPaymentRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesPaymentRefunds_SalesPayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "SalesPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesPaymentRefunds_PaymentId",
                table: "SalesPaymentRefunds",
                column: "PaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SalesPaymentRefunds");

            migrationBuilder.DropColumn(name: "AuthorizationCode", table: "SalesPayments");
            migrationBuilder.DropColumn(name: "Brand", table: "SalesPayments");
            migrationBuilder.DropColumn(name: "Installments", table: "SalesPayments");
            migrationBuilder.DropColumn(name: "Nsu", table: "SalesPayments");
            migrationBuilder.DropColumn(name: "Provider", table: "SalesPayments");
            migrationBuilder.DropColumn(name: "TerminalId", table: "SalesPayments");
        }
    }
}
