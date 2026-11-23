namespace ArabClinic.Application.Invoices;

public sealed record InvoiceDetailsDto(
    int InvoiceId,
    int AppointmentId,
    Guid PatientId,
    string PatientName,
    string DoctorName,
    decimal TotalAmount,
    string Status,
    DateTime IssuedAt,
    decimal PaidAmount,
    decimal RemainingBalance,
    IReadOnlyList<PaymentDto> Payments);

public sealed record PaymentDto(
    int PaymentId,
    decimal Amount,
    string PaymentMethod,
    string PaymentStatus,
    DateTime PaidAt,
    string? TransactionReference);

public sealed record RecordPaymentRequest(
    int InvoiceId,
    decimal Amount,
    string PaymentMethod,
    string? TransactionReference);

public interface IBillingService
{
    Task<IReadOnlyList<InvoiceDetailsDto>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<InvoiceDetailsDto?> GetByIdAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceDetailsDto?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default);
}
