using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddInventoryLossFractionation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "UnitsPerPackage",
            table: "Products",
            type: "TEXT",
            nullable: false,
            defaultValue: 1m);

        migrationBuilder.AddColumn<bool>(
            name: "IsFractional",
            table: "ProductLots",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "Notes",
            table: "StockMovements",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "SupplierId",
            table: "StockMovements",
            type: "TEXT",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "UnitsPerPackage", table: "Products");
        migrationBuilder.DropColumn(name: "IsFractional", table: "ProductLots");
        migrationBuilder.DropColumn(name: "Notes", table: "StockMovements");
        migrationBuilder.DropColumn(name: "SupplierId", table: "StockMovements");
    }
}
