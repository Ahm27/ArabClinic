using ArabClinic.Application.MedicalRecords;
using ArabClinic.Domain.Appointments;
using ArabClinic.Domain.Doctors;
using ArabClinic.Domain.Patients;
using ArabClinic.Domain.Specializations;
using ArabClinic.Domain.Users;
using ArabClinic.Infrastructure.Persistence;
using ArabClinic.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.UnitTests.Infrastructure;

public sealed class MedicalVisitServiceTests
{
    private static ArabClinicDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<ArabClinicDbContext>()
            .UseInMemoryDatabase($"MedicalVisitTests-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task CreatePersistsStandaloneVisitAndHistoryPreservesSubsequentVisits()
    {
        using var db = CreateDbContext();
        var (patient, doctor) = await AddPatientAndDoctorAsync(db);
        var service = new MedicalVisitWriteService(db);

        var first = await service.CreateAsync(new CreateMedicalVisitRequest(
            patient.Id, doctor.DoctorId, null, new DateTime(2026, 1, 10, 9, 0, 0), "First visit summary"));
        var second = await service.CreateAsync(new CreateMedicalVisitRequest(
            patient.Id, doctor.DoctorId, null, new DateTime(2026, 2, 10, 9, 0, 0), "Follow-up summary"));

        Assert.True(first.Success);
        Assert.True(second.Success);
        var readService = new MedicalVisitReadService(db);
        var history = await readService.GetPatientHistoryAsync(patient.Id);
        Assert.Equal(2, history.Count);
        Assert.Equal("Follow-up summary", history[0].Summary);
        Assert.Equal("First visit summary", history[1].Summary);
        Assert.All(history, visit => Assert.Equal(patient.Id, visit.PatientId));
        Assert.Equal(first.MedicalVisitId, history[1].MedicalVisitId);
        Assert.Equal(history[1].CreatedAt, history[1].UpdatedAt);

        var details = await readService.GetByIdAsync(first.MedicalVisitId);
        Assert.NotNull(details);
        Assert.Equal(patient.FullName, details.PatientName);
        Assert.Equal("Dr. Visit Test", details.DoctorName);
        Assert.Equal("First visit summary", details.Summary);
    }

    [Fact]
    public async Task CreateAcceptsMatchingAppointmentReference()
    {
        using var db = CreateDbContext();
        var (patient, doctor) = await AddPatientAndDoctorAsync(db);
        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.DoctorId,
            StartDateTime = new DateTime(2026, 3, 1, 10, 0, 0),
            EndDateTime = new DateTime(2026, 3, 1, 10, 30, 0)
        };
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        var result = await new MedicalVisitWriteService(db).CreateAsync(new CreateMedicalVisitRequest(
            patient.Id, doctor.DoctorId, appointment.AppointmentId, appointment.StartDateTime, "Appointment visit"));

        Assert.True(result.Success);
        var visit = await db.MedicalVisits.SingleAsync();
        Assert.Equal(appointment.AppointmentId, visit.AppointmentId);
        Assert.Equal(patient.Id, visit.PatientId);
        Assert.Equal(doctor.DoctorId, visit.DoctorId);
    }

    [Fact]
    public async Task CreateRejectsAppointmentForDifferentPatientOrDoctor()
    {
        using var db = CreateDbContext();
        var (patient, doctor) = await AddPatientAndDoctorAsync(db);
        var (otherPatient, otherDoctor) = await AddPatientAndDoctorAsync(db, "Other");
        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.DoctorId,
            StartDateTime = DateTime.UtcNow,
            EndDateTime = DateTime.UtcNow.AddMinutes(30)
        };
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        var service = new MedicalVisitWriteService(db);
        var wrongPatient = await service.CreateAsync(new CreateMedicalVisitRequest(
            otherPatient.Id, doctor.DoctorId, appointment.AppointmentId, DateTime.UtcNow, "Mismatch"));
        var wrongDoctor = await service.CreateAsync(new CreateMedicalVisitRequest(
            patient.Id, otherDoctor.DoctorId, appointment.AppointmentId, DateTime.UtcNow, "Mismatch"));

        Assert.False(wrongPatient.Success);
        Assert.False(wrongDoctor.Success);
        Assert.Empty(await db.MedicalVisits.ToListAsync());
    }

    private static async Task<(Patient Patient, Doctor Doctor)> AddPatientAndDoctorAsync(
        ArabClinicDbContext db,
        string suffix = "")
    {
        var user = new ApplicationUser
        {
            Id = $"doctor-{Guid.NewGuid()}{suffix}",
            FullName = "Dr. Visit Test",
            Email = $"doctor-{Guid.NewGuid()}@test.local"
        };
        var specialty = new Specialization { Name = $"Specialty-{Guid.NewGuid()}" };
        var doctor = new Doctor { DoctorId = user.Id, User = user, Specialization = specialty, Location = "Room 1" };
        var patient = new Patient($"Visit Patient {Guid.NewGuid()}", new DateOnly(1990, 1, 1));
        db.AddRange(user, specialty, doctor, patient);
        await db.SaveChangesAsync();
        return (patient, doctor);
    }
}