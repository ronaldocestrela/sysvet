using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPlatformDunningCoupons : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "CardRetryCount",
            table: "PlatformBillingInvoices",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "NextCardRetryAt",
            table: "PlatformBillingInvoices",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "AppliedCouponId",
            table: "PlatformBillingInvoices",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "PastDueSince",
            table: "PlatformTenantSubscriptions",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "PendingCouponId",
            table: "PlatformTenantSubscriptions",
            type: "TEXT",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "PlatformCoupons",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                DiscountType = table.Column<int>(type: "INTEGER", nullable: false),
                Value = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                MaxRedemptions = table.Column<int>(type: "INTEGER", nullable: true),
                RedemptionCount = table.Column<int>(type: "INTEGER", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformCoupons", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PlatformCouponRedemptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CouponId = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                BillingInvoiceId = table.Column<Guid>(type: "TEXT", nullable: true),
                RedeemedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformCouponRedemptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlatformCouponRedemptions_PlatformCoupons_CouponId",
                    column: x => x.CouponId,
                    principalTable: "PlatformCoupons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PlatformDunningNotices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                InvoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                StepDay = table.Column<int>(type: "INTEGER", nullable: false),
                Channel = table.Column<int>(type: "INTEGER", nullable: false),
                ScheduledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                SentAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlatformDunningNotices", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_PlatformCoupons_Code",
            table: "PlatformCoupons",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlatformCouponRedemptions_TenantId_CouponId",
            table: "PlatformCouponRedemptions",
            columns: new[] { "TenantId", "CouponId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlatformDunningNotices_TenantId_InvoiceId_StepDay_Channel",
            table: "PlatformDunningNotices",
            columns: new[] { "TenantId", "InvoiceId", "StepDay", "Channel" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlatformCouponRedemptions");
        migrationBuilder.DropTable(name: "PlatformCoupons");
        migrationBuilder.DropTable(name: "PlatformDunningNotices");

        migrationBuilder.DropColumn(name: "CardRetryCount", table: "PlatformBillingInvoices");
        migrationBuilder.DropColumn(name: "NextCardRetryAt", table: "PlatformBillingInvoices");
        migrationBuilder.DropColumn(name: "AppliedCouponId", table: "PlatformBillingInvoices");
        migrationBuilder.DropColumn(name: "PastDueSince", table: "PlatformTenantSubscriptions");
        migrationBuilder.DropColumn(name: "PendingCouponId", table: "PlatformTenantSubscriptions");
    }
}
