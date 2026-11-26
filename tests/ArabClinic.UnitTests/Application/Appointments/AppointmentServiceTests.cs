using ArabClinic.Application.Appointments;
using ArabClinic.Domain.Appointments;
using ArabClinic.Domain.Doctors;
using ArabClinic.Domain.Patients;
using ArabClinic.Domain.Specializations;
using ArabClinic.Domain.Users;
using ArabClinic.Infrastructure.Persistence;
using ArabClinic.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.UnitTests.Application.Appointments;

public sealed class AppointmentServiceTests
{
    private static ArabClinicDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ArabClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"ArabClinicApptTests-{Guid.NewGuid()}")
            .Options;

        return new ArabClinicDbContext(options);
    }

    [Fact]
    public async Task BookAppointmentFailsWhenEndTimeIsBeforeStartTime()
    {
        using var db = CreateDbContext();
        var service = new AppointmentService(db);

        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddMinutes(-10); // End before start

        var request = new BookAppointmentRequest(
            null,
            "John Doe",
            "john@example.com",
            "+201000000000",
            "doc-1",
            start,
            end,
            "Regular visit");

        var result = await service.BookAppointmentAsync(request);

        Assert.False(result.Success);
        Assert.Contains("end time must be after start time", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BookAppointmentPreventsOverlappingAppointmentsForSameDoctor()
    {
        using var db = CreateDbContext();

        var doctorUser = new ApplicationUser { Id = "doc-101", FullName = "Dr. Test Specialist", Email = "specialist@test.com" };
        var spec = new Specialization { Name = "Cardiology" };
        var doctor = new Doctor { DoctorId = doctorUser.Id, Specialization = spec, ConsultationFee = 300, Location = "Room 1" };
        var patient = new Patient("Sara Test", new DateOnly(1995, 1, 1));

        await db.Users.AddAsync(doctorUser);
        await db.Specializations.AddAsync(spec);
        await db.Doctors.AddAsync(doctor);
        await db.Patients.AddAsync(patient);

        var existingStart = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var existingEnd = existingStart.AddMinutes(30);

        var existingApp = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.DoctorId,
            StartDateTime = existingStart,
            EndDateTime = existingEnd,
            Status = "Scheduled"
        };
        await db.Appointments.AddAsync(existingApp);
        await db.SaveChangesAsync();

        var service = new AppointmentService(db);

        // Attempt to book an overlapping appointment: 10:15 - 10:45
        var overlapRequest = new BookAppointmentRequest(
            patient.Id,
            patient.FullName,
            "sara@test.com",
            "+2011111111",
            doctor.DoctorId,
            existingStart.AddMinutes(15),
            existingEnd.AddMinutes(15),
            "Second checkup");

        var result = await service.BookAppointmentAsync(overlapRequest);

        Assert.False(result.Success);
        Assert.Contains("already has an appointment booked", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BookAppointmentFailsWhenDoctorHasScheduledDayOff()
    {
        using var db = CreateDbContext();

        var doctorUser = new ApplicationUser { Id = "doc-102", FullName = "Dr. DayOff Specialist", Email = "dayoff@test.com" };
        var spec = new Specialization { Name = "Neurology" };
        var doctor = new Doctor { DoctorId = doctorUser.Id, Specialization = spec, ConsultationFee = 400, Location = "Room 2" };

        var dayOffStart = DateTime.UtcNow.Date.AddDays(2).AddHours(8);
        var dayOffEnd = dayOffStart.AddDays(2);
        doctor.DaysOff.Add(new DoctorDayOff { DoctorId = doctor.DoctorId, StartDateTime = dayOffStart, EndDateTime = dayOffEnd, Reason = "Vacation" });

        await db.Users.AddAsync(doctorUser);
        await db.Specializations.AddAsync(spec);
        await db.Doctors.AddAsync(doctor);
        await db.SaveChangesAsync();

        var service = new AppointmentService(db);

        var request = new BookAppointmentRequest(
            null,
            "Guest Patient",
            "guest@test.com",
            "+201000000000",
            doctor.DoctorId,
            dayOffStart.AddHours(2),
            dayOffStart.AddHours(2).AddMinutes(30),
            "Emergency consultation");

        var result = await service.BookAppointmentAsync(request);

        Assert.False(result.Success);
        Assert.Contains("scheduled day off", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BookAppointmentCreatesAppointmentAndAutoGeneratesInvoice()
    {
        using var db = CreateDbContext();

        var doctorUser = new ApplicationUser { Id = "doc-103", FullName = "Dr. Booking Success", Email = "success@test.com" };
        var spec = new Specialization { Name = "Orthopedics" };
        var doctor = new Doctor { DoctorId = doctorUser.Id, Specialization = spec, ConsultationFee = 550, Location = "Room 3" };

        await db.Users.AddAsync(doctorUser);
        await db.Specializations.AddAsync(spec);
        await db.Doctors.AddAsync(doctor);
        await db.SaveChangesAsync();

        var service = new AppointmentService(db);

        var start = DateTime.UtcNow.Date.AddDays(5).AddHours(14);
        var end = start.AddMinutes(30);

        var request = new BookAppointmentRequest(
            null,
            "New Patient",
            "newpatient@test.com",
            "+201234567890",
            doctor.DoctorId,
            start,
            end,
            "Knee consultation");

        var result = await service.BookAppointmentAsync(request);

        Assert.True(result.Success);
        Assert.True(result.AppointmentId > 0);

        var appInDb = await db.Appointments
            .Include(a => a.Invoice)
            .Include(a => a.StatusHistories)
            .FirstOrDefaultAsync(a => a.AppointmentId == result.AppointmentId);

        Assert.NotNull(appInDb);
        Assert.Equal("Scheduled", appInDb.Status);
        Assert.NotNull(appInDb.Invoice);
        Assert.Equal(550, appInDb.Invoice.TotalAmount);
        Assert.Equal("Unpaid", appInDb.Invoice.Status);
        Assert.Single(appInDb.StatusHistories);
    }
}