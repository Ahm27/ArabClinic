using ArabClinic.Application.MedicalRecords;
using ArabClinic.Domain.MedicalRecords;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class MedicalVisitWriteService(ArabClinicDbContext dbContext) : IMedicalVisitWriteService
{
    public async Task<(bool Success, string? ErrorMessage, int MedicalVisitId)> CreateAsync(
        CreateMedicalVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.PatientId == Guid.Empty)
        {
            return (false, "A patient is required.", 0);
        }
        if (string.IsNullOrWhiteSpace(request.DoctorId))
        {
            return (false, "A doctor is required.", 0);
        }
        if (string.IsNullOrWhiteSpace(request.Summary))
        {
            return (false, "A visit summary is required.", 0);
        }

        var patientExists = await dbContext.Patients.AnyAsync(patient => patient.Id == request.PatientId, cancellationToken);
        if (!patientExists)
        {
            return (false, "Patient not found.", 0);
        }

        var doctorExists = await dbContext.Doctors.AnyAsync(doctor => doctor.DoctorId == request.DoctorId, cancellationToken);
        if (!doctorExists)
        {
            return (false, "Doctor not found.", 0);
        }

        if (request.AppointmentId.HasValue)
        {
            var appointmentMatches = await dbContext.Appointments.AnyAsync(appointment =>
                appointment.AppointmentId == request.AppointmentId.Value &&
                appointment.PatientId == request.PatientId &&
                appointment.DoctorId == request.DoctorId,
                cancellationToken);
            if (!appointmentMatches)
            {
                return (false, "The appointment must belong to the selected patient and doctor.", 0);
            }
        }

        var now = DateTime.UtcNow;
        var visit = new MedicalVisit
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId.Trim(),
            AppointmentId = request.AppointmentId,
            VisitDate = request.VisitDate,
            Summary = request.Summary.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await dbContext.MedicalVisits.AddAsync(visit, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, visit.MedicalVisitId);
    }
}