using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineClinicalQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ConversionStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ConvertedOrderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ClinicalQuotes", x => x.Id));

            migrationBuilder.CreateTable(
                name: "ClinicalQuoteItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClinicalQuoteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalQuoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalQuoteItems_ClinicalQuotes_ClinicalQuoteId",
                        column: x => x.ClinicalQuoteId,
                        principalTable: "ClinicalQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_ClinicalQuotes_AppointmentId", table: "ClinicalQuotes", column: "AppointmentId");
            migrationBuilder.CreateIndex(name: "IX_ClinicalQuotes_PetId", table: "ClinicalQuotes", column: "PetId");
            migrationBuilder.CreateIndex(name: "IX_ClinicalQuoteItems_ClinicalQuoteId", table: "ClinicalQuoteItems", column: "ClinicalQuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClinicalQuoteItems");
            migrationBuilder.DropTable(name: "ClinicalQuotes");
        }
    }
}
