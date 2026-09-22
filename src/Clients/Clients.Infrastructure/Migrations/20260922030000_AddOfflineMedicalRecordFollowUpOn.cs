using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <summary>Adds follow-up date on offline medical records (Fase 8.2).</summary>
public partial class AddOfflineMedicalRecordFollowUpOn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateOnly>(
            name: "FollowUpOn",
            table: "MedicalRecords",
            type: "TEXT",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FollowUpOn",
            table: "MedicalRecords");
    }
}
