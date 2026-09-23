using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intelligence.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIntelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "ProfileDashboardLayouts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SlotsJson = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileDashboardLayouts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileDashboardLayouts_AccessProfileId",
                schema: "dbo",
                table: "ProfileDashboardLayouts",
                column: "AccessProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileDashboardLayouts",
                schema: "dbo");
        }
    }
}
