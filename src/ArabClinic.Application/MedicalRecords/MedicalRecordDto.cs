namespace ArabClinic.Application.MedicalRecords;

public sealed record MedicalRecordDetailsDto(
    int MedicalRecordId,
    int AppointmentId,
    Guid PatientId,
    string PatientName,
    string DoctorName,
    DateTime AppointmentDate,
    string Diagnosis,
    string? Symptoms,
    string? TreatmentNotes,
    DateTime CreatedAt,
    IReadOnlyList<PrescriptionDto> Prescriptions,
    IReadOnlyList<MedicalDocumentDto> Documents);

public sealed record PrescriptionDto(
    int PrescriptionId,
    DateTime IssuedAt,
    IReadOnlyList<PrescriptionItemDto> Items);

public sealed record PrescriptionItemDto(
    int ItemId,
    string MedicationName,
    string Dosage,
    string Frequency,
    string Duration);

public sealed record MedicalDocumentDto(
    int DocumentId,
    string FileName,
    string StorageKey,
    string ContentType,
    DateTime UploadedAt);

public sealed record CreateMedicalRecordRequest(
    int AppointmentId,
    string Diagnosis,
    string? Symptoms,
    string? TreatmentNotes,
    List<CreatePrescriptionItemRequest>? PrescriptionItems);

public sealed record CreatePrescriptionItemRequest(
    string MedicationName,
    string Dosage,
    string Frequency,
    string Duration);

public interface IMedicalRecordService
{
    Task<MedicalRecordDetailsDto?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<MedicalRecordDetailsDto?> GetByIdAsync(int medicalRecordId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage, int MedicalRecordId)> CreateAsync(CreateMedicalRecordRequest request, CancellationToken cancellationToken = default);
}
