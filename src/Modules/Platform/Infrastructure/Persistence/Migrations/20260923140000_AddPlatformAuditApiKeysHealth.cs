using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPlatformAuditApiKeysHealth : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlatformLoginLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                Succeeded = table.Column<bool>(type: "INTEGER", nullable: false),
                ClientIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                UserAgent = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                Country = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Region = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformLoginLogs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformChangeAuditEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ActorUserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                Action = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                PayloadSummary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                ClientIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformChangeAuditEntries", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformPartnerApiKeys",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                PartnerName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                KeyPrefix = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                SecretHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Scope = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                CreatedByUserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RevokedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformPartnerApiKeys", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformTenantRequestDailies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                DateUtc = table.Column<DateOnly>(type: "TEXT", nullable: false),
                RequestCount = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformTenantRequestDailies", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_PlatformLoginLogs_OccurredAt",
            table: "PlatformLoginLogs",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformLoginLogs_TenantId",
            table: "PlatformLoginLogs",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformChangeAuditEntries_OccurredAt",
            table: "PlatformChangeAuditEntries",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformChangeAuditEntries_TenantId",
            table: "PlatformChangeAuditEntries",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformPartnerApiKeys_SecretHash",
            table: "PlatformPartnerApiKeys",
            column: "SecretHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlatformPartnerApiKeys_TenantId_CreatedAt",
            table: "PlatformPartnerApiKeys",
            columns: new[] { "TenantId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_PlatformTenantRequestDailies_TenantId_DateUtc",
            table: "PlatformTenantRequestDailies",
            columns: new[] { "TenantId", "DateUtc" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformLoginLogs");
        migrationBuilder.DropTable(name: "PlatformChangeAuditEntries");
        migrationBuilder.DropTable(name: "PlatformPartnerApiKeys");
        migrationBuilder.DropTable(name: "PlatformTenantRequestDailies");
    }
}
