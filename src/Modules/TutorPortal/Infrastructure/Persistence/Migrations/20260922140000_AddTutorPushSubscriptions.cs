using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPortal.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorPushSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TutorPushSubscriptions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    P256dh = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Auth = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorPushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorPushSubscriptions_UserId_Endpoint",
                schema: "dbo",
                table: "TutorPushSubscriptions",
                columns: new[] { "UserId", "Endpoint" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorPushSubscriptions",
                schema: "dbo");
        }
    }
}
