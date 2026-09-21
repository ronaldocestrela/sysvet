using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineTutorPostalAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                table: "Tutors",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressComplement",
                table: "Tutors",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressDistrict",
                table: "Tutors",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AddressIbgeCityCode",
                table: "Tutors",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                table: "Tutors",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressPostalCode",
                table: "Tutors",
                type: "TEXT",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressState",
                table: "Tutors",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                table: "Tutors",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalIntegrationStatus",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCity",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressComplement",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressDistrict",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressIbgeCityCode",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressNumber",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressPostalCode",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressState",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "FiscalIntegrationStatus",
                table: "SalesOrders");
        }
    }
}
