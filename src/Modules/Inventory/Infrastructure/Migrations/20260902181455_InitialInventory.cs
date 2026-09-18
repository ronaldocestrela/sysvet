using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "dbo");

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Document = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Suppliers", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    SupplierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Ncm = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Cest = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    MerchandiseOrigin = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    RequiresLot = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Products", x => x.Id));

            migrationBuilder.CreateTable(
                name: "ProductBalances",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBalances_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductLots",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LotNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    UnitCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLots_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductLotId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AdjustmentDirection = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_ProductBalances_ProductId", schema: "dbo", table: "ProductBalances", column: "ProductId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_ProductBalances_TenantId_ProductId", schema: "dbo", table: "ProductBalances", columns: new[] { "TenantId", "ProductId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ProductLots_ProductId", schema: "dbo", table: "ProductLots", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_ProductLots_TenantId_ProductId_LotNumber", schema: "dbo", table: "ProductLots", columns: new[] { "TenantId", "ProductId", "LotNumber" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Products_TenantId_Barcode", schema: "dbo", table: "Products", columns: new[] { "TenantId", "Barcode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Products_TenantId_Sku", schema: "dbo", table: "Products", columns: new[] { "TenantId", "Sku" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_StockMovements_ProductId", schema: "dbo", table: "StockMovements", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_Suppliers_TenantId_Document", schema: "dbo", table: "Suppliers", columns: new[] { "TenantId", "Document" }, unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductLots", schema: "dbo");
            migrationBuilder.DropTable(name: "StockMovements", schema: "dbo");
            migrationBuilder.DropTable(name: "ProductBalances", schema: "dbo");
            migrationBuilder.DropTable(name: "Products", schema: "dbo");
            migrationBuilder.DropTable(name: "Suppliers", schema: "dbo");
        }
    }
}
