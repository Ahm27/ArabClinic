namespace ArabClinic.Domain.Notifications;

public class NotificationDeliveryLog
{
    public int DeliveryLogId { get; set; }
    public int NotificationId { get; set; }
    public string Channel { get; set; } = "Email";
    public string DeliveryStatus { get; set; } = "Sent";
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Notification? Notification { get; set; }
}
