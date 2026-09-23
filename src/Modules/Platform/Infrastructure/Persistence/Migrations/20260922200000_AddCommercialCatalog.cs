using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCommercialCatalog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlatformPlans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                MonthlyPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformPlans", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformAddOns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                MonthlyPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                Module = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformAddOns", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformPlanModules",
            columns: table => new
            {
                PlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                Module = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformPlanModules", x => new { x.PlanId, x.Module });
                table.ForeignKey(
                    name: "FK_PlatformPlanModules_PlatformPlans_PlanId",
                    column: x => x.PlanId,
                    principalTable: "PlatformPlans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PlatformTenantSubscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                PlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                PeriodStart = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                PeriodEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TrialEndsAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TrialEndAction = table.Column<int>(type: "INTEGER", nullable: false),
                CreditBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformTenantSubscriptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlatformTenantSubscriptions_PlatformPlans_PlanId",
                    column: x => x.PlanId,
                    principalTable: "PlatformPlans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlatformFeatureFlags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Module = table.Column<int>(type: "INTEGER", nullable: false),
                State = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformFeatureFlags", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformSubscriptionAdjustments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                FromPlanId = table.Column<Guid>(type: "TEXT", nullable: true),
                ToPlanId = table.Column<Guid>(type: "TEXT", nullable: true),
                AddOnId = table.Column<Guid>(type: "TEXT", nullable: true),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformSubscriptionAdjustments", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformTenantAddOns",
            columns: table => new
            {
                SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                AddOnId = table.Column<Guid>(type: "TEXT", nullable: false),
                ActivatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformTenantAddOns", x => new { x.SubscriptionId, x.AddOnId });
                table.ForeignKey(
                    name: "FK_PlatformTenantAddOns_PlatformAddOns_AddOnId",
                    column: x => x.AddOnId,
                    principalTable: "PlatformAddOns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlatformTenantAddOns_PlatformTenantSubscriptions_SubscriptionId",
                    column: x => x.SubscriptionId,
                    principalTable: "PlatformTenantSubscriptions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_PlatformPlans_Code", table: "PlatformPlans", column: "Code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PlatformAddOns_Code", table: "PlatformAddOns", column: "Code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PlatformTenantSubscriptions_TenantId", table: "PlatformTenantSubscriptions", column: "TenantId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PlatformTenantSubscriptions_PlanId", table: "PlatformTenantSubscriptions", column: "PlanId");
        migrationBuilder.CreateIndex(name: "IX_PlatformFeatureFlags_TenantId_Module", table: "PlatformFeatureFlags", columns: new[] { "TenantId", "Module" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_PlatformSubscriptionAdjustments_TenantId", table: "PlatformSubscriptionAdjustments", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_PlatformTenantAddOns_AddOnId", table: "PlatformTenantAddOns", column: "AddOnId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformTenantAddOns");
        migrationBuilder.DropTable(name: "PlatformFeatureFlags");
        migrationBuilder.DropTable(name: "PlatformPlanModules");
        migrationBuilder.DropTable(name: "PlatformSubscriptionAdjustments");
        migrationBuilder.DropTable(name: "PlatformTenantSubscriptions");
        migrationBuilder.DropTable(name: "PlatformAddOns");
        migrationBuilder.DropTable(name: "PlatformPlans");
    }
}
