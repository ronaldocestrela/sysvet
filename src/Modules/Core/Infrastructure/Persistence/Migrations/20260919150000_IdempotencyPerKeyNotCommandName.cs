using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [Migration("20260919150000_IdempotencyPerKeyNotCommandName")]
    public partial class IdempotencyPerKeyNotCommandName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdempotencyRecords_TenantId_Name",
                schema: "dbo",
                table: "IdempotencyRecords");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_TenantId_Name",
                schema: "dbo",
                table: "IdempotencyRecords",
                columns: new[] { "TenantId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdempotencyRecords_TenantId_Name",
                schema: "dbo",
                table: "IdempotencyRecords");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_TenantId_Name",
                schema: "dbo",
                table: "IdempotencyRecords",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }
    }
}
