using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCardReconciliationAndAllocationNsu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                schema: "dbo",
                table: "TitleAllocations",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CardReconciliationBatches",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardReconciliationBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardReconciliationLines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nsu = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Fee = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MatchedAllocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardReconciliationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardReconciliationLines_CardReconciliationBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "dbo",
                        principalTable: "CardReconciliationBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TitleAllocations_ExternalReference",
                schema: "dbo",
                table: "TitleAllocations",
                column: "ExternalReference");

            migrationBuilder.CreateIndex(
                name: "IX_CardReconciliationLines_BatchId",
                schema: "dbo",
                table: "CardReconciliationLines",
                column: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardReconciliationLines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CardReconciliationBatches",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_TitleAllocations_ExternalReference",
                schema: "dbo",
                table: "TitleAllocations");

            migrationBuilder.DropColumn(
                name: "ExternalReference",
                schema: "dbo",
                table: "TitleAllocations");
        }
    }
}
