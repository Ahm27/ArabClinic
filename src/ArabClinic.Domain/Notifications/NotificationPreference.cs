namespace ArabClinic.Domain.Notifications;

public class NotificationPreference
{
    public string UserId { get; set; } = string.Empty; // PK/FK -> Users.Id
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = true;

    public Users.ApplicationUser? User { get; set; }
}