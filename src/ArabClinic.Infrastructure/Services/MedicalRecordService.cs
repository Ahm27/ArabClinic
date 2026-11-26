using ArabClinic.Application.MedicalRecords;
using ArabClinic.Domain.MedicalRecords;
using ArabClinic.Domain.Notifications;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class MedicalRecordService(ArabClinicDbContext dbContext) : IMedicalRecordService
{
    public async Task<MedicalRecordDetailsDto?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Appointment)
                .ThenInclude(a => a!.Patient)
            .Include(m => m.Appointment)
                .ThenInclude(a => a!.Doctor)
                    .ThenInclude(d => d!.User)
            .Include(m => m.Prescriptions)
                .ThenInclude(p => p.Items)
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId, cancellationToken);

        return record == null ? null : Map(record);
    }

    public async Task<MedicalRecordDetailsDto?> GetByIdAsync(int medicalRecordId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Appointment)
                .ThenInclude(a => a!.Patient)
            .Include(m => m.Appointment)
                .ThenInclude(a => a!.Doctor)
                    .ThenInclude(d => d!.User)
            .Include(m => m.Prescriptions)
                .ThenInclude(p => p.Items)
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId, cancellationToken);

        return record == null ? null : Map(record);
    }

    public async Task<(bool Success, string? ErrorMessage, int MedicalRecordId)> CreateAsync(
        CreateMedicalRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var app = await dbContext.Appointments
            .Include(a => a.MedicalRecord)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId, cancellationToken);

        if (app == null)
        {
            return (false, "Appointment not found.", 0);
        }

        if (app.MedicalRecord != null)
        {
            return (false, "A medical record already exists for this appointment.", app.MedicalRecord.MedicalRecordId);
        }

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            Diagnosis = request.Diagnosis.Trim(),
            Symptoms = request.Symptoms?.Trim(),
            TreatmentNotes = request.TreatmentNotes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        if (request.PrescriptionItems != null && request.PrescriptionItems.Count > 0)
        {
            var prescription = new Prescription
            {
                IssuedAt = DateTime.UtcNow
            };

            foreach (var item in request.PrescriptionItems)
            {
                prescription.Items.Add(new PrescriptionItem
                {
                    MedicationName = item.MedicationName.Trim(),
                    Dosage = item.Dosage.Trim(),
                    Frequency = item.Frequency.Trim(),
                    Duration = item.Duration.Trim()
                });
            }

            record.Prescriptions.Add(prescription);
        }

        // Set appointment status to Completed
        app.Status = "Completed";

        await dbContext.MedicalRecords.AddAsync(record, cancellationToken);

        // Notify patient
        await dbContext.Notifications.AddAsync(new Notification
        {
            UserId = app.PatientId.ToString(),
            Message = $"Doctor {app.Doctor?.User?.FullName ?? "Physician"} has completed your medical record for appointment #{app.AppointmentId}.",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return (true, null, record.MedicalRecordId);
    }

    private static MedicalRecordDetailsDto Map(MedicalRecord m) =>
        new(
            m.MedicalRecordId,
            m.AppointmentId,
            m.Appointment?.PatientId ?? Guid.Empty,
            m.Appointment?.Patient?.FullName ?? "Patient",
            m.Appointment?.Doctor?.User?.FullName ?? "Doctor",
            m.Appointment?.StartDateTime ?? m.CreatedAt,
            m.Diagnosis,
            m.Symptoms,
            m.TreatmentNotes,
            m.CreatedAt,
            m.Prescriptions.Select(p => new PrescriptionDto(
                p.PrescriptionId,
                p.IssuedAt,
                p.Items.Select(i => new PrescriptionItemDto(i.ItemId, i.MedicationName, i.Dosage, i.Frequency, i.Duration)).ToList()
            )).ToList(),
            m.Documents.Select(d => new MedicalDocumentDto(d.DocumentId, d.FileName, d.StorageKey, d.ContentType, d.UploadedAt)).ToList()
        );
}