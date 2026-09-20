using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePrepaidUsageMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrepaidUsages",
                schema: "dbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrepaidUsages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AttendanceRef = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrepaidBalanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepaidUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrepaidUsages_PrepaidBalances_PrepaidBalanceId",
                        column: x => x.PrepaidBalanceId,
                        principalSchema: "dbo",
                        principalTable: "PrepaidBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrepaidUsages_PrepaidBalanceId",
                schema: "dbo",
                table: "PrepaidUsages",
                column: "PrepaidBalanceId");
        }
    }
}
