namespace ArabClinic.Domain.Notifications;

public class Notification
{
    public int NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Users.ApplicationUser? User { get; set; }
    public ICollection<NotificationDeliveryLog> DeliveryLogs { get; set; } = [];
}