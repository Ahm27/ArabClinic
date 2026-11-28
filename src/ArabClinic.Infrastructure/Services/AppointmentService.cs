using ArabClinic.Application.Appointments;
using ArabClinic.Domain.Appointments;
using ArabClinic.Domain.Invoices;
using ArabClinic.Domain.Notifications;
using ArabClinic.Domain.Patients;
using ArabClinic.Domain.Users;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class AppointmentService(ArabClinicDbContext dbContext) : IAppointmentService
{
    public async Task<IReadOnlyList<AppointmentDto>> GetAllAsync(
        string? status = null,
        string? doctorId = null,
        Guid? patientId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d!.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d!.Specialization)
            .Include(a => a.MedicalRecord)
            .Include(a => a.Invoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var trimmedStatus = status.Trim();
            query = query.Where(a => a.Status == trimmedStatus);
        }

        if (!string.IsNullOrWhiteSpace(doctorId))
        {
            query = query.Where(a => a.DoctorId == doctorId);
        }

        if (patientId.HasValue && patientId.Value != Guid.Empty)
        {
            query = query.Where(a => a.PatientId == patientId.Value);
        }

        var appointments = await query
            .OrderByDescending(a => a.StartDateTime)
            .ToListAsync(cancellationToken);

        return appointments.Select(a => new AppointmentDto(
            a.AppointmentId,
            a.PatientId,
            a.Patient?.FullName ?? "Unknown Patient",
            a.DoctorId,
            a.Doctor?.User?.FullName ?? "Doctor",
            a.Doctor?.Specialization?.Name ?? "General",
            a.StartDateTime,
            a.EndDateTime,
            a.Status,
            a.Notes,
            a.CreatedAt,
            a.Doctor?.ConsultationFee ?? 0,
            a.MedicalRecord != null,
            a.Invoice != null,
            a.Invoice?.Status,
            a.Invoice?.TotalAmount
        )).ToList();
    }

    public async Task<AppointmentDetailsDto?> GetByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var a = await dbContext.Appointments
            .AsNoTracking()
            .Include(x => x.Patient)
                .ThenInclude(p => p!.User)
            .Include(x => x.Doctor)
                .ThenInclude(d => d!.User)
            .Include(x => x.Doctor)
                .ThenInclude(d => d!.Specialization)
            .Include(x => x.StatusHistories)
                .ThenInclude(h => h.ChangedByUser)
            .Include(x => x.MedicalRecord)
                .ThenInclude(m => m!.Prescriptions)
            .Include(x => x.MedicalRecord)
                .ThenInclude(m => m!.Documents)
            .Include(x => x.Invoice)
                .ThenInclude(i => i!.Payments)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId, cancellationToken);

        if (a == null)
        {
            return null;
        }

        var medSummary = a.MedicalRecord == null ? null : new MedicalRecordSummaryDto(
            a.MedicalRecord.MedicalRecordId,
            a.MedicalRecord.Diagnosis,
            a.MedicalRecord.Symptoms,
            a.MedicalRecord.TreatmentNotes,
            a.MedicalRecord.CreatedAt,
            a.MedicalRecord.Prescriptions.Count,
            a.MedicalRecord.Documents.Count
        );

        var invSummary = a.Invoice == null ? null : new InvoiceSummaryDto(
            a.Invoice.InvoiceId,
            a.Invoice.TotalAmount,
            a.Invoice.Status,
            a.Invoice.IssuedAt,
            a.Invoice.Payments.Sum(p => p.Amount)
        );

        return new AppointmentDetailsDto(
            a.AppointmentId,
            a.PatientId,
            a.Patient?.FullName ?? "Unknown",
            a.Patient?.User?.Email,
            a.Patient?.User?.PhoneNumber,
            a.DoctorId,
            a.Doctor?.User?.FullName ?? "Doctor",
            a.Doctor?.Specialization?.Name ?? "General",
            a.Doctor?.Location ?? "Clinic",
            a.Doctor?.ConsultationFee ?? 0,
            a.StartDateTime,
            a.EndDateTime,
            a.Status,
            a.Notes,
            a.CreatedAt,
            a.StatusHistories
                .OrderBy(h => h.ChangedAt)
                .Select(h => new StatusHistoryDto(h.HistoryId, h.OldStatus, h.NewStatus, h.ChangedByUser?.FullName ?? "System", h.ChangedAt))
                .ToList(),
            medSummary,
            invSummary
        );
    }

    public async Task<(bool Success, string? ErrorMessage, int AppointmentId)> BookAppointmentAsync(
        BookAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var start = request.StartDateTime.Kind == DateTimeKind.Utc
            ? request.StartDateTime
            : request.StartDateTime.ToUniversalTime();
        if (request.EndDateTime.HasValue && request.EndDateTime.Value <= request.StartDateTime)
            return (false, "Appointment end time must be after start time.", 0);
        if (start <= DateTime.UtcNow) return (false, "Appointments must be booked in the future.", 0);

        var doctor = await dbContext.Doctors
            .Include(d => d.User)
            .Include(d => d.Schedules)
            .Include(d => d.DaysOff)
            .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId, cancellationToken);

        if (doctor == null || !doctor.IsApproved)
        {
            return (false, "Selected doctor was not found.", 0);
        }

        var allowedDurations = new[] { 15, 20, 30, 45, 60 };
        var duration = allowedDurations.Contains(doctor.SlotDurationMinutes) ? doctor.SlotDurationMinutes : 30;
        var end = start.AddMinutes(duration);
        var cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(start, cairo);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(end, cairo);
        // Check Doctor Days Off
        var isDayOff = doctor.DaysOff.Any(d =>
            start < d.EndDateTime && end > d.StartDateTime);

        if (isDayOff)
        {
            return (false, $"{doctor.User?.FullName ?? "The doctor"} is unavailable during this time (scheduled day off).", 0);
        }

        if (doctor.Schedules.Count > 0)
        {
            var schedule = doctor.Schedules.FirstOrDefault(item => item.DayOfWeek == localStart.DayOfWeek);
            if (schedule is null || localStart.TimeOfDay < schedule.StartTime || localEnd.TimeOfDay > schedule.EndTime)
                return (false, "The selected time is outside the doctor's working hours.", 0);
        }

        // Check for overlapping appointments (including concurrent requests)
        var hasOverlap = await dbContext.Appointments
            .AnyAsync(a =>
                a.DoctorId == request.DoctorId &&
                a.Status != "Cancelled" &&
                start < a.EndDateTime &&
                end > a.StartDateTime,
                cancellationToken);

        if (hasOverlap)
        {
            return (false, $"{doctor.User?.FullName ?? "The doctor"} already has an appointment booked in that time window. Please select another slot.", 0);
        }

        // Patient resolution: existing or create from booking form
        Guid patientId;
        if (request.PatientId.HasValue && request.PatientId.Value != Guid.Empty)
        {
            patientId = request.PatientId.Value;
            if (!await dbContext.Patients.AnyAsync(patient => patient.Id == patientId && !patient.IsArchived, cancellationToken))
                return (false, "Patient profile was not found.", 0);
        }
        else
        {
            var pName = string.IsNullOrWhiteSpace(request.PatientName) ? "Patient Guest" : request.PatientName.Trim();
            var patient = new Patient(pName, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)));

            var user = new ApplicationUser
            {
                Id = patient.Id.ToString(),
                FullName = pName,
                Email = string.IsNullOrWhiteSpace(request.PatientEmail) ? $"{patient.Id}@guest.arabclinic.com" : request.PatientEmail.Trim(),
                PhoneNumber = request.PatientPhone?.Trim(),
                UserName = string.IsNullOrWhiteSpace(request.PatientEmail) ? patient.Id.ToString() : request.PatientEmail.Trim()
            };

            patient.UserId = user.Id;
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.Patients.AddAsync(patient, cancellationToken);
            patientId = patient.Id;
        }

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = request.DoctorId,
            StartDateTime = start,
            EndDateTime = end,
            Status = "Scheduled",
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        appointment.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = null,
            NewStatus = "Scheduled",
            ChangedByUserId = request.DoctorId,
            ChangedAt = DateTime.UtcNow
        });

        // Automatically create Invoice with Doctor Consultation Fee
        appointment.Invoice = new Invoice
        {
            TotalAmount = doctor.ConsultationFee,
            Status = "Unpaid",
            IssuedAt = DateTime.UtcNow
        };

        await dbContext.Appointments.AddAsync(appointment, cancellationToken);

        // Notification for patient
        await dbContext.Notifications.AddAsync(new Notification
        {
            UserId = patientId.ToString(),
            Message = $"Your appointment with {doctor.User?.FullName ?? "Doctor"} has been scheduled for {start:MMM dd, yyyy HH:mm}.",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return (false, "This slot was just booked. Please select another available time.", 0);
        }

        return (true, null, appointment.AppointmentId);
    }

    public async Task<bool> UpdateStatusAsync(int appointmentId, string newStatus, string changedByUserId, CancellationToken cancellationToken = default)
    {
        var app = await dbContext.Appointments
            .Include(a => a.StatusHistories)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

        if (app == null)
        {
            return false;
        }

        var allowed = app.Status switch
        {
            "Pending" => new[] { "Confirmed", "Rejected", "Cancelled" },
            "Scheduled" => new[] { "Confirmed", "Completed", "Cancelled" },
            "Confirmed" => new[] { "Completed", "Cancelled" },
            _ => Array.Empty<string>()
        };
        if (!allowed.Contains(newStatus, StringComparer.Ordinal)) return false;
        if (!await dbContext.Users.AnyAsync(user => user.Id == changedByUserId, cancellationToken)) return false;

        var old = app.Status;
        app.Status = newStatus;

        app.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = old,
            NewStatus = newStatus,
            ChangedByUserId = string.IsNullOrWhiteSpace(changedByUserId) ? app.DoctorId : changedByUserId,
            ChangedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CancelAsync(int appointmentId, string changedByUserId, CancellationToken cancellationToken = default)
    {
        return await UpdateStatusAsync(appointmentId, "Cancelled", changedByUserId, cancellationToken);
    }
}