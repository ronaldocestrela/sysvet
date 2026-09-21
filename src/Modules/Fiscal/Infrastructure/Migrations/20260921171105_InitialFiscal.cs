using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiscal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "FiscalDocuments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SourceOrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessKey = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    Protocol = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    XmlBlobKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DanfeBlobKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NfeNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    NfeSeries = table.Column<int>(type: "INTEGER", nullable: true),
                    NfseNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AuthorizedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RecipientName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RecipientCpf = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    RecipientUf = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssuerProfiles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    StateRegistration = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MunicipalRegistration = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TaxRegimeCode = table.Column<int>(type: "INTEGER", nullable: false),
                    Cnae = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Street = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Complement = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    IbgeCityCode = table.Column<int>(type: "INTEGER", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NationalServiceTaxCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DefaultIssRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    NfeSeries = table.Column<int>(type: "INTEGER", nullable: false),
                    NextNfeNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    DpsSeries = table.Column<int>(type: "INTEGER", nullable: false),
                    NextDpsNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    Environment = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CertificateBlobKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EncryptedCertificatePassword = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuerProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiscalCorrectionLetters",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FiscalDocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    CorrectionText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Protocol = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    TransmittedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalCorrectionLetters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiscalCorrectionLetters_FiscalDocuments_FiscalDocumentId",
                        column: x => x.FiscalDocumentId,
                        principalSchema: "dbo",
                        principalTable: "FiscalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiscalDocumentItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FiscalDocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Ncm = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    Cfop = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    Csosn = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    MerchandiseOrigin = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalDocumentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiscalDocumentItems_FiscalDocuments_FiscalDocumentId",
                        column: x => x.FiscalDocumentId,
                        principalSchema: "dbo",
                        principalTable: "FiscalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalCorrectionLetters_FiscalDocumentId",
                schema: "dbo",
                table: "FiscalCorrectionLetters",
                column: "FiscalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocumentItems_FiscalDocumentId",
                schema: "dbo",
                table: "FiscalDocumentItems",
                column: "FiscalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocuments_SourceOrderId_DocumentType_Status",
                schema: "dbo",
                table: "FiscalDocuments",
                columns: new[] { "SourceOrderId", "DocumentType", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiscalCorrectionLetters",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FiscalDocumentItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "IssuerProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FiscalDocuments",
                schema: "dbo");
        }
    }
}
