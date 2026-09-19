using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OfflineSnapshotSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "SalesOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SellerUserId",
                table: "SalesOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PerformerRole",
                table: "SalesOrderItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PerformerUserId",
                table: "SalesOrderItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnedQuantity",
                table: "SalesOrderItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SalesCommissionAccruals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PayeeUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    RatePercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    BaseCurrency = table.Column<string>(type: "TEXT", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CommissionCurrency = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCommissionAccruals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesCommissionAccruals_SalesOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesCommissionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    AppliesTo = table.Column<string>(type: "TEXT", nullable: false),
                    RatePercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCommissionRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesSaleReturns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    RefundCurrency = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesSaleReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesSaleReturns_SalesOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesSaleReturnLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SaleReturnId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesSaleReturnLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesSaleReturnLines_SalesSaleReturns_SaleReturnId",
                        column: x => x.SaleReturnId,
                        principalTable: "SalesSaleReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissionAccruals_OrderId",
                table: "SalesCommissionAccruals",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissionRules_Role_AppliesTo",
                table: "SalesCommissionRules",
                columns: new[] { "Role", "AppliesTo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesSaleReturnLines_SaleReturnId",
                table: "SalesSaleReturnLines",
                column: "SaleReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesSaleReturns_OrderId",
                table: "SalesSaleReturns",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesCommissionAccruals");

            migrationBuilder.DropTable(
                name: "SalesCommissionRules");

            migrationBuilder.DropTable(
                name: "SalesSaleReturnLines");

            migrationBuilder.DropTable(
                name: "SalesSaleReturns");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SellerUserId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "PerformerRole",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "PerformerUserId",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "SalesOrderItems");
        }
    }
}
