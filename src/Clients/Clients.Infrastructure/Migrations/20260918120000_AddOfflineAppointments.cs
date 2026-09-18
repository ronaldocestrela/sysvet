using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Infrastructure.Migrations;

/// <inheritdoc />
[Migration("20260918120000_AddOfflineAppointments")]
public partial class AddOfflineAppointments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Appointments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                PetId = table.Column<Guid>(type: "TEXT", nullable: false),
                VeterinarianId = table.Column<Guid>(type: "TEXT", nullable: false),
                Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                DurationInMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Appointments", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ScheduleSlots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                VeterinarianId = table.Column<Guid>(type: "TEXT", nullable: false),
                Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ScheduleSlots", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Appointments");
        migrationBuilder.DropTable(name: "ScheduleSlots");
    }
}
