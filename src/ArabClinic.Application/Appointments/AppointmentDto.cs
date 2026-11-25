namespace ArabClinic.Application.Appointments;

public sealed record AppointmentDto(
    int AppointmentId,
    Guid PatientId,
    string PatientName,
    string DoctorId,
    string DoctorName,
    string SpecializationName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    decimal ConsultationFee,
    bool HasMedicalRecord,
    bool HasInvoice,
    string? InvoiceStatus,
    decimal? InvoiceAmount);

public sealed record AppointmentDetailsDto(
    int AppointmentId,
    Guid PatientId,
    string PatientName,
    string? PatientEmail,
    string? PatientPhone,
    string DoctorId,
    string DoctorName,
    string SpecializationName,
    string DoctorLocation,
    decimal ConsultationFee,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<StatusHistoryDto> StatusHistories,
    MedicalRecordSummaryDto? MedicalRecord,
    InvoiceSummaryDto? Invoice);

public sealed record StatusHistoryDto(
    int HistoryId,
    string? OldStatus,
    string NewStatus,
    string ChangedByUserName,
    DateTime ChangedAt);

public sealed record MedicalRecordSummaryDto(
    int MedicalRecordId,
    string Diagnosis,
    string? Symptoms,
    string? TreatmentNotes,
    DateTime CreatedAt,
    int PrescriptionCount,
    int DocumentCount);

public sealed record InvoiceSummaryDto(
    int InvoiceId,
    decimal TotalAmount,
    string Status,
    DateTime IssuedAt,
    decimal PaidAmount);

public sealed record BookAppointmentRequest(
    Guid? PatientId,
    string? PatientName,
    string? PatientEmail,
    string? PatientPhone,
    string DoctorId,
    DateTime StartDateTime,
    DateTime? EndDateTime,
    string? Notes);

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentDto>> GetAllAsync(string? status = null, string? doctorId = null, Guid? patientId = null, CancellationToken cancellationToken = default);
    Task<AppointmentDetailsDto?> GetByIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage, int AppointmentId)> BookAppointmentAsync(BookAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int appointmentId, string newStatus, string changedByUserId, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(int appointmentId, string changedByUserId, CancellationToken cancellationToken = default);
}
