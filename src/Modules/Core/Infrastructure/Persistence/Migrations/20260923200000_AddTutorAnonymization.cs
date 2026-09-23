using System;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CoreDbContext))]
    [Migration("20260923200000_AddTutorAnonymization")]
    public partial class AddTutorAnonymization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AnonymizedAt",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymized",
                schema: "dbo",
                table: "Tutors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnonymizedAt",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "IsAnonymized",
                schema: "dbo",
                table: "Tutors");
        }
    }
}
