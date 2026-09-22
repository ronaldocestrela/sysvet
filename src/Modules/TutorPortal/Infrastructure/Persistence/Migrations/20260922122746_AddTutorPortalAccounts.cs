using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPortal.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorPortalAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "TutorPortalAccounts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorPortalAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorPortalAccounts_TutorId",
                schema: "dbo",
                table: "TutorPortalAccounts",
                column: "TutorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorPortalAccounts_UserId",
                schema: "dbo",
                table: "TutorPortalAccounts",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorPortalAccounts",
                schema: "dbo");
        }
    }
}
