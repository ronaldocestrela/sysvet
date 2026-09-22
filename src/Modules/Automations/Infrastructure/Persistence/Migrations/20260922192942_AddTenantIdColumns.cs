using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "TutorMessagingPreferences",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "NpsInvites",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "MessageTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "Campaigns",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "CampaignRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "AutomationsSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "TutorMessagingPreferences");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "NpsInvites");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "MessageTemplates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "CampaignRuns");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "AutomationsSettings");
        }
    }
}
