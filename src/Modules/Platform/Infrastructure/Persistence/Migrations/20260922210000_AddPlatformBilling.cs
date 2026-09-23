using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPlatformBilling : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "BillingStanding",
            table: "PlatformTenantSubscriptions",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<Guid>(
            name: "BillingInvoiceId",
            table: "PlatformSubscriptionAdjustments",
            type: "TEXT",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlatformSubscriptionAdjustments_BillingInvoiceId",
            table: "PlatformSubscriptionAdjustments",
            column: "BillingInvoiceId");

        migrationBuilder.CreateTable(
            name: "PlatformBillingCustomers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                CpfCnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                GatewayCustomerId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformBillingCustomers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformBillingPaymentMethods",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Kind = table.Column<int>(type: "INTEGER", nullable: false),
                CreditCardToken = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformBillingPaymentMethods", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformBillingInvoices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                PeriodStart = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                PeriodEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                PaidAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformBillingInvoices", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformBillingWebhookReceipts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                ProcessedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformBillingWebhookReceipts", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformBillingCharges",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                InvoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                GatewayPaymentId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                PixCopyPaste = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                BoletoIdentificationField = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformBillingCharges", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlatformBillingCharges_PlatformBillingInvoices_InvoiceId",
                    column: x => x.InvoiceId,
                    principalTable: "PlatformBillingInvoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_PlatformBillingCustomers_TenantId", table: "PlatformBillingCustomers", column: "TenantId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PlatformBillingPaymentMethods_TenantId", table: "PlatformBillingPaymentMethods", column: "TenantId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PlatformBillingInvoices_TenantId", table: "PlatformBillingInvoices", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_PlatformBillingInvoices_TenantId_Status", table: "PlatformBillingInvoices", columns: new[] { "TenantId", "Status" });
        migrationBuilder.CreateIndex(name: "IX_PlatformBillingWebhookReceipts_IdempotencyKey", table: "PlatformBillingWebhookReceipts", column: "IdempotencyKey", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PlatformBillingCharges_InvoiceId", table: "PlatformBillingCharges", column: "InvoiceId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformBillingCharges");
        migrationBuilder.DropTable(name: "PlatformBillingWebhookReceipts");
        migrationBuilder.DropTable(name: "PlatformBillingInvoices");
        migrationBuilder.DropTable(name: "PlatformBillingPaymentMethods");
        migrationBuilder.DropTable(name: "PlatformBillingCustomers");
        migrationBuilder.DropIndex(name: "IX_PlatformSubscriptionAdjustments_BillingInvoiceId", table: "PlatformSubscriptionAdjustments");
        migrationBuilder.DropColumn(name: "BillingInvoiceId", table: "PlatformSubscriptionAdjustments");
        migrationBuilder.DropColumn(name: "BillingStanding", table: "PlatformTenantSubscriptions");
    }
}
