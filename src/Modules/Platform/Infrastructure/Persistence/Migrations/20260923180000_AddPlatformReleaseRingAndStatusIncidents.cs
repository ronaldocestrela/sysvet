using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPlatformReleaseRingAndStatusIncidents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ReleaseRing",
            table: "PlatformTenants",
            type: "INTEGER",
            nullable: false,
            defaultValue: 2);

        migrationBuilder.CreateTable(
            name: "PlatformStatusIncidents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                Impact = table.Column<int>(type: "INTEGER", nullable: false),
                Components = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                ResolvedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformStatusIncidents", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlatformStatusIncidents_ResolvedAt",
            table: "PlatformStatusIncidents",
            column: "ResolvedAt");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformStatusIncidents_StartedAt",
            table: "PlatformStatusIncidents",
            column: "StartedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformStatusIncidents");
        migrationBuilder.DropColumn(name: "ReleaseRing", table: "PlatformTenants");
    }
}
