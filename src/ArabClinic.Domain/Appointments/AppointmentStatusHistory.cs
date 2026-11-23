namespace ArabClinic.Domain.Appointments;

public class AppointmentStatusHistory
{
    public int HistoryId { get; set; }
    public int AppointmentId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string ChangedByUserId { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public Appointment? Appointment { get; set; }
    public Users.ApplicationUser? ChangedByUser { get; set; }
}
