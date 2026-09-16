using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogEntityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId_OccurredAt",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                schema: "dbo",
                table: "AuditLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_EntityId_OccurredAt",
                schema: "dbo",
                table: "AuditLogs",
                columns: new[] { "TenantId", "EntityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_EntityName_OccurredAt",
                schema: "dbo",
                table: "AuditLogs",
                columns: new[] { "TenantId", "EntityName", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId_EntityId_OccurredAt",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId_EntityName_OccurredAt",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_OccurredAt",
                schema: "dbo",
                table: "AuditLogs",
                columns: new[] { "TenantId", "OccurredAt" });
        }
    }
}
