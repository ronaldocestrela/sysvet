using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignsNps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarketingEnabled",
                schema: "dbo",
                table: "TutorMessagingPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Campaigns",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SegmentKind = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    TemplateCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InactiveDays = table.Column<int>(type: "INTEGER", nullable: false),
                    CooldownDays = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Campaigns", x => x.Id));

            migrationBuilder.CreateTable(
                name: "CampaignRuns",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CampaignId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AudienceCount = table.Column<int>(type: "INTEGER", nullable: false),
                    EnqueuedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRuns_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalSchema: "dbo",
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NpsInvites",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: true),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RespondedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CampaignId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_NpsInvites", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRuns_CampaignId",
                schema: "dbo",
                table: "CampaignRuns",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_NpsInvites_SourceType_SourceId",
                schema: "dbo",
                table: "NpsInvites",
                columns: new[] { "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NpsInvites_TutorId",
                schema: "dbo",
                table: "NpsInvites",
                column: "TutorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CampaignRuns", schema: "dbo");
            migrationBuilder.DropTable(name: "NpsInvites", schema: "dbo");
            migrationBuilder.DropTable(name: "Campaigns", schema: "dbo");
            migrationBuilder.DropColumn(name: "MarketingEnabled", schema: "dbo", table: "TutorMessagingPreferences");
        }
    }
}
