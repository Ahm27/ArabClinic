using Microsoft.AspNetCore.Identity;

namespace ArabClinic.Domain.Users;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    // Navigations
    public Doctors.Doctor? Doctor { get; set; }
    public Patients.Patient? Patient { get; set; }
    public Notifications.NotificationPreference? NotificationPreference { get; set; }
    public ICollection<Notifications.Notification> Notifications { get; set; } = [];
}