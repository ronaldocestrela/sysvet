using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalQuotes",
                schema: "dbo",
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
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClinicalQuoteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
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
                        principalSchema: "dbo",
                        principalTable: "ClinicalQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalQuotes_AppointmentId",
                schema: "dbo",
                table: "ClinicalQuotes",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalQuotes_PetId",
                schema: "dbo",
                table: "ClinicalQuotes",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalQuotes_ConversionStatus",
                schema: "dbo",
                table: "ClinicalQuotes",
                column: "ConversionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalQuoteItems_ClinicalQuoteId",
                schema: "dbo",
                table: "ClinicalQuoteItems",
                column: "ClinicalQuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClinicalQuoteItems", schema: "dbo");
            migrationBuilder.DropTable(name: "ClinicalQuotes", schema: "dbo");
        }
    }
}
