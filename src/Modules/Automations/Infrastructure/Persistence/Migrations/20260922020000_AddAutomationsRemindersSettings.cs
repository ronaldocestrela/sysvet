using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationsRemindersSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationsSettings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    BusinessStart = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    BusinessEnd = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    BusinessDaysJson = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AutomationsSettings", x => x.Id));

            migrationBuilder.CreateTable(
                name: "TutorMessagingPreferences",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WhatsAppEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_TutorMessagingPreferences", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_AutomationsSettings_Key",
                schema: "dbo",
                table: "AutomationsSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorMessagingPreferences_TutorId",
                schema: "dbo",
                table: "TutorMessagingPreferences",
                column: "TutorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AutomationsSettings", schema: "dbo");
            migrationBuilder.DropTable(name: "TutorMessagingPreferences", schema: "dbo");
        }
    }
}
