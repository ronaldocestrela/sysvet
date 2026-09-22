using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Commerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommerce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "MarketplaceSellerIndexes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MercadoLivreUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceSellerIndexes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceSyncJobs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductOfferId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceSyncJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MercadoLivreSettings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: true),
                    SiteId = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MercadoLivreSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnlineOrders",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Channel = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Fulfillment = table.Column<int>(type: "INTEGER", nullable: false),
                    BuyerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BuyerPhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    BuyerEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ExternalOrderId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductOffers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SalePrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    StoreEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MercadoLivreEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExternalListingId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnlineOrderLines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OnlineOrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductOfferId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineOrderLines_OnlineOrders_OnlineOrderId",
                        column: x => x.OnlineOrderId,
                        principalSchema: "dbo",
                        principalTable: "OnlineOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceSellerIndexes_MercadoLivreUserId",
                schema: "dbo",
                table: "MarketplaceSellerIndexes",
                column: "MercadoLivreUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceSyncJobs_IdempotencyKey",
                schema: "dbo",
                table: "MarketplaceSyncJobs",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_MercadoLivreSettings_Key",
                schema: "dbo",
                table: "MercadoLivreSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrderLines_OnlineOrderId",
                schema: "dbo",
                table: "OnlineOrderLines",
                column: "OnlineOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_ExternalOrderId",
                schema: "dbo",
                table: "OnlineOrders",
                column: "ExternalOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffers_ProductId",
                schema: "dbo",
                table: "ProductOffers",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceSellerIndexes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MarketplaceSyncJobs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MercadoLivreSettings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OnlineOrderLines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductOffers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OnlineOrders",
                schema: "dbo");
        }
    }
}
