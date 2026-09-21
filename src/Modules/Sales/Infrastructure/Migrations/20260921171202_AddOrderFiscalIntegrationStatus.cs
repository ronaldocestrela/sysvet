using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderFiscalIntegrationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FiscalIntegrationStatus",
                schema: "dbo",
                table: "Orders",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConsumerCpf",
                schema: "dbo",
                table: "Orders",
                type: "TEXT",
                maxLength: 14,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiscalIntegrationStatus",
                schema: "dbo",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ConsumerCpf",
                schema: "dbo",
                table: "Orders");
        }
    }
}
