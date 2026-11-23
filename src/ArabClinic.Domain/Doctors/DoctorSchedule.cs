namespace ArabClinic.Domain.Doctors;

public class DoctorSchedule
{
    public int ScheduleId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public Doctor? Doctor { get; set; }
}
