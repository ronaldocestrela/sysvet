using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOfflineSalesPdv : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SalesCashRegisters",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OpenedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                OpenedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                ClosedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                OpeningBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                OpeningCurrency = table.Column<string>(type: "TEXT", nullable: false),
                ClosingBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                ClosingCurrency = table.Column<string>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_SalesCashRegisters", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SalesOrders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CashRegisterId = table.Column<Guid>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                TutorId = table.Column<Guid>(type: "TEXT", nullable: true),
                PetId = table.Column<Guid>(type: "TEXT", nullable: true),
                SourceQuoteId = table.Column<Guid>(type: "TEXT", nullable: true),
                FinanceIntegrationStatus = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                PaidAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_SalesOrders", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SalesOrderItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                Kind = table.Column<string>(type: "TEXT", nullable: false),
                ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                ProductName = table.Column<string>(type: "TEXT", nullable: false),
                Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                Currency = table.Column<string>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrderItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_SalesOrderItems_SalesOrders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "SalesOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SalesPayments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                Method = table.Column<string>(type: "TEXT", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                Currency = table.Column<string>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesPayments", x => x.Id);
                table.ForeignKey(
                    name: "FK_SalesPayments_SalesOrders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "SalesOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderItems_OrderId",
            table: "SalesOrderItems",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_SalesPayments_OrderId",
            table: "SalesPayments",
            column: "OrderId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SalesOrderItems");
        migrationBuilder.DropTable(name: "SalesPayments");
        migrationBuilder.DropTable(name: "SalesOrders");
        migrationBuilder.DropTable(name: "SalesCashRegisters");
    }
}
