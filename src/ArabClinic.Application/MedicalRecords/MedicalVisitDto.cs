namespace ArabClinic.Application.MedicalRecords;

public sealed record MedicalVisitDto(
    int MedicalVisitId,
    Guid PatientId,
    string PatientName,
    string DoctorId,
    string DoctorName,
    int? AppointmentId,
    DateTime VisitDate,
    string Summary,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateMedicalVisitRequest(
    Guid PatientId,
    string DoctorId,
    int? AppointmentId,
    DateTime VisitDate,
    string Summary);

public interface IMedicalVisitReadService
{
    Task<IReadOnlyList<MedicalVisitDto>> GetPatientHistoryAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<MedicalVisitDto?> GetByIdAsync(int medicalVisitId, CancellationToken cancellationToken = default);
}

public interface IMedicalVisitWriteService
{
    Task<(bool Success, string? ErrorMessage, int MedicalVisitId)> CreateAsync(
        CreateMedicalVisitRequest request,
        CancellationToken cancellationToken = default);
}
