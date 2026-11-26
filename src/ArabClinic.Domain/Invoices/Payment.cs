namespace ArabClinic.Domain.Invoices;

public class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Credit Card"; // Credit Card, Cash, Insurance
    public string PaymentStatus { get; set; } = "Completed"; // Completed, Pending, Failed
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? TransactionReference { get; set; }

    public Invoice? Invoice { get; set; }
}