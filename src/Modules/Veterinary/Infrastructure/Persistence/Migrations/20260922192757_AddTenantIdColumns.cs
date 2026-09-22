using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "WardUnits",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "VaccineProtocols",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "VaccineProtocolDoses",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "VaccineDoses",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ScheduleSlots",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "PrescriptionTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "PrescriptionTemplateItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "PrescriptionItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "MedicationAdministrations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "MedicalRecords",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "IssuedPrescriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "HospitalProcedures",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "HospitalMedicationOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "Hospitalizations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "HospitalizationProgressNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "EvolutionNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalQuotes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalQuoteItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalExams",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalAttachments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "Beds",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "dbo",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "WardUnits");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "VaccineProtocols");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "VaccineProtocolDoses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "VaccineDoses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "PrescriptionTemplates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "PrescriptionTemplateItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "MedicationAdministrations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "IssuedPrescriptions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "HospitalProcedures");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "HospitalMedicationOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "Hospitalizations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "HospitalizationProgressNotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "EvolutionNotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalQuotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalQuoteItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalExams");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "ClinicalAttachments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "Appointments");
        }
    }
}
