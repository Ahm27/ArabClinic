using ArabClinic.Application.Invoices;
using ArabClinic.Domain.Invoices;
using ArabClinic.Domain.Notifications;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class BillingService(ArabClinicDbContext dbContext) : IBillingService
{
    public async Task<IReadOnlyList<InvoiceDetailsDto>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Invoices
            .AsNoTracking()
            .Include(i => i.Payments)
            .Include(i => i.Appointment)
                .ThenInclude(a => a!.Patient)
            .Include(i => i.Appointment)
                .ThenInclude(a => a!.Doctor)
                    .ThenInclude(d => d!.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var trimmedStatus = status.Trim();
            query = query.Where(i => i.Status == trimmedStatus);
        }

        var invoices = await query
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync(cancellationToken);

        return invoices.Select(Map).ToList();
    }

    public async Task<InvoiceDetailsDto?> GetByIdAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(i => i.Payments)
            .Include(i => i.Appointment)
                .ThenInclude(a => a!.Patient)
            .Include(i => i.Appointment)
                .ThenInclude(a => a!.Doctor)
                    .ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        return invoice == null ? null : Map(invoice);
    }

    public async Task<InvoiceDetailsDto?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(i => i.Payments)
            .Include(i => i.Appointment)
                .ThenInclude(a => a!.Patient)
            .Include(i => i.Appointment)
                .ThenInclude(a => a!.Doctor)
                    .ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId, cancellationToken);

        return invoice == null ? null : Map(invoice);
    }

    public async Task<(bool Success, string? ErrorMessage)> RecordPaymentAsync(
        RecordPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Amount <= 0)
        {
            return (false, "Payment amount must be greater than zero.");
        }

        var invoice = await dbContext.Invoices
            .Include(i => i.Payments)
            .Include(i => i.Appointment)
                .ThenInclude(a => a!.Patient)
            .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            return (false, "Invoice was not found.");
        }

        var paidSoFar = invoice.Payments.Sum(p => p.Amount);
        var remaining = invoice.TotalAmount - paidSoFar;

        if (request.Amount > remaining)
        {
            return (false, $"Payment amount ({request.Amount:C}) exceeds remaining invoice balance ({remaining:C}).");
        }

        var payment = new Payment
        {
            InvoiceId = invoice.InvoiceId,
            Amount = request.Amount,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod.Trim(),
            PaymentStatus = "Completed",
            PaidAt = DateTime.UtcNow,
            TransactionReference = request.TransactionReference ?? $"TXN-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}"
        };

        invoice.Payments.Add(payment);

        if (paidSoFar + request.Amount >= invoice.TotalAmount)
        {
            invoice.Status = "Paid";
        }
        else
        {
            invoice.Status = "PartiallyPaid";
        }

        // Notify patient
        if (invoice.Appointment != null)
        {
            await dbContext.Notifications.AddAsync(new Notification
            {
                UserId = invoice.Appointment.PatientId.ToString(),
                Message = $"Payment of {request.Amount:C} received for invoice #{invoice.InvoiceId}. Current status: {invoice.Status}.",
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private static InvoiceDetailsDto Map(Invoice i)
    {
        var paid = i.Payments.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount);
        var remaining = Math.Max(0, i.TotalAmount - paid);

        return new InvoiceDetailsDto(
            i.InvoiceId,
            i.AppointmentId,
            i.Appointment?.PatientId ?? Guid.Empty,
            i.Appointment?.Patient?.FullName ?? "Patient",
            i.Appointment?.Doctor?.User?.FullName ?? "Doctor",
            i.TotalAmount,
            i.Status,
            i.IssuedAt,
            paid,
            remaining,
            i.Payments.Select(p => new PaymentDto(
                p.PaymentId,
                p.Amount,
                p.PaymentMethod,
                p.PaymentStatus,
                p.PaidAt,
                p.TransactionReference
            )).OrderByDescending(p => p.PaidAt).ToList()
        );
    }
}
