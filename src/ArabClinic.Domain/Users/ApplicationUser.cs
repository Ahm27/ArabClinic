namespace ArabClinic.Domain.Users;

public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool EmailConfirmed { get; set; } = true;
    public bool PhoneNumberConfirmed { get; set; } = true;
    public string? SecurityStamp { get; set; } = Guid.NewGuid().ToString();

    // Navigations
    public Doctors.Doctor? Doctor { get; set; }
    public Patients.Patient? Patient { get; set; }
    public Notifications.NotificationPreference? NotificationPreference { get; set; }
    public ICollection<Notifications.Notification> Notifications { get; set; } = [];
}
