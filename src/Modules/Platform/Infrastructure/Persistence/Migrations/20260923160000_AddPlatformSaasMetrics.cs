using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPlatformSaasMetrics : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CancelledAt",
            table: "PlatformTenants",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "RefundedAt",
            table: "PlatformBillingInvoices",
            type: "TEXT",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE PlatformTenants
            SET CancelledAt = UpdatedAt
            WHERE Status IN (2, 3) AND CancelledAt IS NULL
            """);

        migrationBuilder.CreateTable(
            name: "PlatformAcquisitionSpends",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Year = table.Column<int>(type: "INTEGER", nullable: false),
                Month = table.Column<int>(type: "INTEGER", nullable: false),
                Channel = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                Note = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformAcquisitionSpends", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_PlatformAcquisitionSpends_Year_Month_Channel",
            table: "PlatformAcquisitionSpends",
            columns: new[] { "Year", "Month", "Channel" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformAcquisitionSpends");
        migrationBuilder.DropColumn(name: "RefundedAt", table: "PlatformBillingInvoices");
        migrationBuilder.DropColumn(name: "CancelledAt", table: "PlatformTenants");
    }
}
