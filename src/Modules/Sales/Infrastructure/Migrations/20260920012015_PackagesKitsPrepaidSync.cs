using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PackagesKitsPrepaidSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CatalogOfferId",
                schema: "dbo",
                table: "OrderItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrepaidBalances",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RemainingUses = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchasedUses = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepaidBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductKits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductKits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ServiceCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UsesPerUnit = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrepaidCredits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrepaidBalanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsesCredited = table.Column<int>(type: "INTEGER", nullable: false),
                    UsesRemaining = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepaidCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrepaidCredits_PrepaidBalances_PrepaidBalanceId",
                        column: x => x.PrepaidBalanceId,
                        principalSchema: "dbo",
                        principalTable: "PrepaidBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrepaidUsages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrepaidBalanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AttendanceRef = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepaidUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrepaidUsages_PrepaidBalances_PrepaidBalanceId",
                        column: x => x.PrepaidBalanceId,
                        principalSchema: "dbo",
                        principalTable: "PrepaidBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KitComponents",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductKitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuantityPerKit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitComponents_ProductKits_ProductKitId",
                        column: x => x.ProductKitId,
                        principalSchema: "dbo",
                        principalTable: "ProductKits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KitComponents_ProductKitId",
                schema: "dbo",
                table: "KitComponents",
                column: "ProductKitId");

            migrationBuilder.CreateIndex(
                name: "IX_PrepaidBalances_TenantId_PetId_ServiceCode",
                schema: "dbo",
                table: "PrepaidBalances",
                columns: new[] { "TenantId", "PetId", "ServiceCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrepaidCredits_OrderItemId",
                schema: "dbo",
                table: "PrepaidCredits",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrepaidCredits_PrepaidBalanceId",
                schema: "dbo",
                table: "PrepaidCredits",
                column: "PrepaidBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrepaidUsages_PrepaidBalanceId",
                schema: "dbo",
                table: "PrepaidUsages",
                column: "PrepaidBalanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KitComponents",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PrepaidCredits",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PrepaidUsages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ServicePackages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductKits",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PrepaidBalances",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "CatalogOfferId",
                schema: "dbo",
                table: "OrderItems");
        }
    }
}
