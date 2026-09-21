using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorPostalAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressComplement",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressDistrict",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AddressIbgeCityCode",
                schema: "dbo",
                table: "Tutors",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressPostalCode",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressState",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCity",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressComplement",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressDistrict",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressIbgeCityCode",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressNumber",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressPostalCode",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressState",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                schema: "dbo",
                table: "Tutors");
        }
    }
}
