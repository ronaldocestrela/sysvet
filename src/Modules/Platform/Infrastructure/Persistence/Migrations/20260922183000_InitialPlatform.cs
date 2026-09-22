using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialPlatform : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlatformTenants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Slug = table.Column<string>(type: "TEXT", maxLength: 63, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                SchemaName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformTenants", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PlatformBranches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Cnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                LegalName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                IsHeadquarters = table.Column<bool>(type: "INTEGER", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformBranches", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlatformTenants_Slug",
            table: "PlatformTenants",
            column: "Slug");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformBranches_TenantId",
            table: "PlatformBranches",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformBranches_TenantId_Cnpj",
            table: "PlatformBranches",
            columns: new[] { "TenantId", "Cnpj" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformBranches");
        migrationBuilder.DropTable(name: "PlatformTenants");
    }
}
