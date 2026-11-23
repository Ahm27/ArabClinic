namespace ArabClinic.Domain.Invoices;

public class Invoice
{
    public int InvoiceId { get; set; }
    public int AppointmentId { get; set; } // FK UNIQUE
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, PartiallyPaid, Cancelled
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public Appointments.Appointment? Appointment { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];
}
