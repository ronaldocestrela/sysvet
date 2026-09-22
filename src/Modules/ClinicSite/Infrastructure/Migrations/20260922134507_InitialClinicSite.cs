using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialClinicSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "ClinicSiteProfiles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tagline = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Street = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Complement = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    WhatsApp = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 63, nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicSiteProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicSiteSlugs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 63, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicSiteSlugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicSiteOpeningHours",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClinicSiteProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    CloseTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicSiteOpeningHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicSiteOpeningHours_ClinicSiteProfiles_ClinicSiteProfileId",
                        column: x => x.ClinicSiteProfileId,
                        principalSchema: "dbo",
                        principalTable: "ClinicSiteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicSiteServices",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClinicSiteProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicSiteServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicSiteServices_ClinicSiteProfiles_ClinicSiteProfileId",
                        column: x => x.ClinicSiteProfileId,
                        principalSchema: "dbo",
                        principalTable: "ClinicSiteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicSiteTeamMembers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClinicSiteProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    RoleTitle = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicSiteTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicSiteTeamMembers_ClinicSiteProfiles_ClinicSiteProfileId",
                        column: x => x.ClinicSiteProfileId,
                        principalSchema: "dbo",
                        principalTable: "ClinicSiteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSiteOpeningHours_ClinicSiteProfileId",
                schema: "dbo",
                table: "ClinicSiteOpeningHours",
                column: "ClinicSiteProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSiteProfiles_Key",
                schema: "dbo",
                table: "ClinicSiteProfiles",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSiteServices_ClinicSiteProfileId",
                schema: "dbo",
                table: "ClinicSiteServices",
                column: "ClinicSiteProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSiteSlugs_Slug",
                schema: "dbo",
                table: "ClinicSiteSlugs",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSiteSlugs_TenantId",
                schema: "dbo",
                table: "ClinicSiteSlugs",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSiteTeamMembers_ClinicSiteProfileId",
                schema: "dbo",
                table: "ClinicSiteTeamMembers",
                column: "ClinicSiteProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicSiteOpeningHours",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClinicSiteServices",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClinicSiteSlugs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClinicSiteTeamMembers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClinicSiteProfiles",
                schema: "dbo");
        }
    }
}
