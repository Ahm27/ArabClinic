namespace ArabClinic.Domain.Doctors;

public class Doctor
{
    public string DoctorId { get; set; } = string.Empty;
    public int SpecializationId { get; set; }
    public decimal ConsultationFee { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;

    // Navigations
    public Users.ApplicationUser? User { get; set; }
    public Specializations.Specialization? Specialization { get; set; }
    public ICollection<DoctorSchedule> Schedules { get; set; } = [];
    public ICollection<DoctorDayOff> DaysOff { get; set; } = [];
    public ICollection<Appointments.Appointment> Appointments { get; set; } = [];
}
