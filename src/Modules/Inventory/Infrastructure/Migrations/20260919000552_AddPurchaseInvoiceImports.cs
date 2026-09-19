using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseInvoiceImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceImports",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessKey = table.Column<string>(type: "TEXT", maxLength: 44, nullable: false),
                    SupplierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EmitterLegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EmitterTradeName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EmitterDocument = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    InvoiceSeries = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ApIntegrationStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BlobKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierProductMappings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SupplierId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SupplierProductCode = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierProductMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceImportLines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PurchaseInvoiceImportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplierProductCode = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Ncm = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    LotNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProductLotId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StockMovementId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceImportLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceImportLines_PurchaseInvoiceImports_PurchaseInvoiceImportId",
                        column: x => x.PurchaseInvoiceImportId,
                        principalSchema: "dbo",
                        principalTable: "PurchaseInvoiceImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceImportLines_PurchaseInvoiceImportId",
                schema: "dbo",
                table: "PurchaseInvoiceImportLines",
                column: "PurchaseInvoiceImportId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceImports_TenantId_AccessKey",
                schema: "dbo",
                table: "PurchaseInvoiceImports",
                columns: new[] { "TenantId", "AccessKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProductMappings_TenantId_SupplierId_SupplierProductCode",
                schema: "dbo",
                table: "SupplierProductMappings",
                columns: new[] { "TenantId", "SupplierId", "SupplierProductCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseInvoiceImportLines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SupplierProductMappings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceImports",
                schema: "dbo");
        }
    }
}
