using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSite.Infrastructure.Migrations
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
                table: "ClinicSiteTeamMembers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicSiteServices",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicSiteProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicSiteOpeningHours",
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
                table: "ClinicSiteTeamMembers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicSiteServices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicSiteProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicSiteOpeningHours");
        }
    }
}
