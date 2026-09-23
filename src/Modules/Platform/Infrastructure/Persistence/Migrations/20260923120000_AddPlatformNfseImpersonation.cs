using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPlatformNfseImpersonation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "City",
            table: "PlatformBranches",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "District",
            table: "PlatformBranches",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "IbgeCode",
            table: "PlatformBranches",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PostalCode",
            table: "PlatformBranches",
            type: "TEXT",
            maxLength: 8,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StateCode",
            table: "PlatformBranches",
            type: "TEXT",
            maxLength: 2,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Street",
            table: "PlatformBranches",
            type: "TEXT",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StreetNumber",
            table: "PlatformBranches",
            type: "TEXT",
            maxLength: 32,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "PlatformImpersonationAuditEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                ActorUserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                TargetTenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Action = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                ClientIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformImpersonationAuditEntries", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformImpersonationSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ActorUserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                ActorEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                TargetTenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                ClientIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                EndedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformImpersonationSessions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformSaasServiceInvoices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                BillingInvoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                RecipientCnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                RecipientLegalName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                NfseNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                AccessKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                XmlBlobKey = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                FailureReason = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformSaasServiceInvoices", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_PlatformImpersonationAuditEntries_OccurredAt",
            table: "PlatformImpersonationAuditEntries",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformImpersonationAuditEntries_SessionId",
            table: "PlatformImpersonationAuditEntries",
            column: "SessionId");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformImpersonationSessions_ExpiresAt",
            table: "PlatformImpersonationSessions",
            column: "ExpiresAt");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformImpersonationSessions_TargetTenantId",
            table: "PlatformImpersonationSessions",
            column: "TargetTenantId");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformSaasServiceInvoices_BillingInvoiceId",
            table: "PlatformSaasServiceInvoices",
            column: "BillingInvoiceId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlatformSaasServiceInvoices_TenantId",
            table: "PlatformSaasServiceInvoices",
            column: "TenantId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformImpersonationAuditEntries");
        migrationBuilder.DropTable(name: "PlatformImpersonationSessions");
        migrationBuilder.DropTable(name: "PlatformSaasServiceInvoices");

        migrationBuilder.DropColumn(name: "City", table: "PlatformBranches");
        migrationBuilder.DropColumn(name: "District", table: "PlatformBranches");
        migrationBuilder.DropColumn(name: "IbgeCode", table: "PlatformBranches");
        migrationBuilder.DropColumn(name: "PostalCode", table: "PlatformBranches");
        migrationBuilder.DropColumn(name: "StateCode", table: "PlatformBranches");
        migrationBuilder.DropColumn(name: "Street", table: "PlatformBranches");
        migrationBuilder.DropColumn(name: "StreetNumber", table: "PlatformBranches");
    }
}
