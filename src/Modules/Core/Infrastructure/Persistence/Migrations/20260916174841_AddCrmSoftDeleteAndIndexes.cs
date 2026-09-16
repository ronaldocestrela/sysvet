using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmSoftDeleteAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Tutors_TutorId",
                schema: "dbo",
                table: "Pets");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "dbo",
                table: "Tutors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Tutors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "dbo",
                table: "Pets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Pets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tutors_Name",
                schema: "dbo",
                table: "Tutors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_Name",
                schema: "dbo",
                table: "Pets",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Tutors_TutorId",
                schema: "dbo",
                table: "Pets",
                column: "TutorId",
                principalSchema: "dbo",
                principalTable: "Tutors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Tutors_TutorId",
                schema: "dbo",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_Tutors_Name",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropIndex(
                name: "IX_Pets_Name",
                schema: "dbo",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "dbo",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "Pets");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Tutors_TutorId",
                schema: "dbo",
                table: "Pets",
                column: "TutorId",
                principalSchema: "dbo",
                principalTable: "Tutors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
