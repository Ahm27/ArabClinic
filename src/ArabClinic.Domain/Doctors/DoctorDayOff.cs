namespace ArabClinic.Domain.Doctors;

public class DoctorDayOff
{
    public int DayOffId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Reason { get; set; }

    public Doctor? Doctor { get; set; }
}