using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <summary>Adds LGPD anonymization columns to offline tutors (Fase 10.6).</summary>
public partial class AddOfflineTutorAnonymization : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "AnonymizedAt",
            table: "Tutors",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsAnonymized",
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
            table: "Tutors");

        migrationBuilder.DropColumn(
            name: "IsAnonymized",
            table: "Tutors");
    }
}
