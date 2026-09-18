using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOfflinePrescriptionTemplates : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PrescriptionTemplates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Species = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PrescriptionTemplates", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PrescriptionTemplateItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                PrescriptionTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                MedicationName = table.Column<string>(type: "TEXT", nullable: false),
                Concentration = table.Column<string>(type: "TEXT", nullable: false),
                Dose = table.Column<string>(type: "TEXT", nullable: false),
                Route = table.Column<string>(type: "TEXT", nullable: false),
                Frequency = table.Column<string>(type: "TEXT", nullable: false),
                Duration = table.Column<string>(type: "TEXT", nullable: false),
                Instructions = table.Column<string>(type: "TEXT", nullable: false),
                SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PrescriptionTemplateItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_PrescriptionTemplateItems_PrescriptionTemplates_PrescriptionTemplateId",
                    column: x => x.PrescriptionTemplateId,
                    principalTable: "PrescriptionTemplates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PrescriptionTemplateItems_PrescriptionTemplateId",
            table: "PrescriptionTemplateItems",
            column: "PrescriptionTemplateId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PrescriptionTemplateItems");
        migrationBuilder.DropTable(name: "PrescriptionTemplates");
    }
}
