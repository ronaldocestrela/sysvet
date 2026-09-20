using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesPackagesKitsPrepaidOffline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CatalogOfferId",
                table: "SalesOrderItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesPrepaidBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceCode = table.Column<string>(type: "TEXT", nullable: false),
                    RemainingUses = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchasedUses = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPrepaidBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesProductKits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesProductKits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesServicePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ServiceCode = table.Column<string>(type: "TEXT", nullable: false),
                    UsesPerUnit = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesServicePackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesKitComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductKitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuantityPerKit = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesKitComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesKitComponents_SalesProductKits_ProductKitId",
                        column: x => x.ProductKitId,
                        principalTable: "SalesProductKits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesKitComponents_ProductKitId",
                table: "SalesKitComponents",
                column: "ProductKitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesKitComponents");

            migrationBuilder.DropTable(
                name: "SalesPrepaidBalances");

            migrationBuilder.DropTable(
                name: "SalesServicePackages");

            migrationBuilder.DropTable(
                name: "SalesProductKits");

            migrationBuilder.DropColumn(
                name: "CatalogOfferId",
                table: "SalesOrderItems");
        }
    }
}
