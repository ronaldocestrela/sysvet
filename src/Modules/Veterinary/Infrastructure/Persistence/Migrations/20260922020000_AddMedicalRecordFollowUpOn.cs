using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordFollowUpOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FollowUpOn",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowUpOn",
                schema: "dbo",
                table: "MedicalRecords");
        }
    }
}
