using System;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(InventoryDbContext))]
    [Migration("20260919120000_AddLossFractionationFields")]
    public partial class AddLossFractionationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitsPerPackage",
                schema: "dbo",
                table: "Products",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<bool>(
                name: "IsFractional",
                schema: "dbo",
                table: "ProductLots",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "dbo",
                table: "StockMovements",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                schema: "dbo",
                table: "StockMovements",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitsPerPackage",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFractional",
                schema: "dbo",
                table: "ProductLots");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "dbo",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                schema: "dbo",
                table: "StockMovements");
        }
    }
}
