using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOfflineFiscalNfce : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FiscalIssuerCache",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                LegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                TradeName = table.Column<string>(type: "TEXT", nullable: false),
                Cnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                IbgeCityCode = table.Column<int>(type: "INTEGER", nullable: false),
                NfceSeries = table.Column<int>(type: "INTEGER", nullable: false),
                HasCertificate = table.Column<bool>(type: "INTEGER", nullable: false),
                EncryptedPfxBase64 = table.Column<string>(type: "TEXT", nullable: true),
                EncryptedCertificatePassword = table.Column<string>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_FiscalIssuerCache", x => x.Id));

        migrationBuilder.CreateTable(
            name: "FiscalSequences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                NfceSeries = table.Column<int>(type: "INTEGER", nullable: false),
                NextNfceNumber = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_FiscalSequences", x => x.Id));

        migrationBuilder.CreateTable(
            name: "FiscalDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                DocumentType = table.Column<string>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                AccessKey = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                Protocol = table.Column<string>(type: "TEXT", nullable: true),
                QrCodeUrl = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                SignedXml = table.Column<string>(type: "TEXT", nullable: true),
                NfeNumber = table.Column<int>(type: "INTEGER", nullable: true),
                NfeSeries = table.Column<int>(type: "INTEGER", nullable: true),
                RecipientName = table.Column<string>(type: "TEXT", nullable: false),
                RecipientCpf = table.Column<string>(type: "TEXT", nullable: true),
                EmissionType = table.Column<string>(type: "TEXT", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                AuthorizedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_FiscalDocuments", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_FiscalDocuments_OrderId",
            table: "FiscalDocuments",
            column: "OrderId");

        migrationBuilder.AddColumn<string>(
            name: "ConsumerCpf",
            table: "SalesOrders",
            type: "TEXT",
            maxLength: 14,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "FiscalDocuments");
        migrationBuilder.DropTable(name: "FiscalSequences");
        migrationBuilder.DropTable(name: "FiscalIssuerCache");
        migrationBuilder.DropColumn(name: "ConsumerCpf", table: "SalesOrders");
    }
}
