using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArabClinic.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ArabClinicDbContext))]
[Migration("20261126120000_AddMedicalVisits")]
public sealed class AddMedicalVisits : Migration
{

    private static readonly string[] PatientIdVisitDateIndexColumns = ["PatientId", "VisitDate"];
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MedicalVisits",
            columns: table => new
            {
                MedicalVisitId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                AppointmentId = table.Column<int>(type: "int", nullable: true),
                VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MedicalVisits", x => x.MedicalVisitId);
                table.ForeignKey("FK_MedicalVisits_Appointments_AppointmentId", x => x.AppointmentId, "Appointments", "AppointmentId", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_MedicalVisits_Doctors_DoctorId", x => x.DoctorId, "Doctors", "DoctorId", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_MedicalVisits_Patients_PatientId", x => x.PatientId, "Patients", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_MedicalVisits_AppointmentId", "MedicalVisits", "AppointmentId");
        migrationBuilder.CreateIndex("IX_MedicalVisits_DoctorId", "MedicalVisits", "DoctorId");
        migrationBuilder.CreateIndex("IX_MedicalVisits_PatientId_VisitDate", "MedicalVisits", PatientIdVisitDateIndexColumns);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MedicalVisits");
    }
}