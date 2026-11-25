namespace ArabClinic.Domain.Appointments;

public class Appointment
{
    public int AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Completed, Cancelled
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigations
    public Patients.Patient? Patient { get; set; }
    public Doctors.Doctor? Doctor { get; set; }
    public MedicalRecords.MedicalRecord? MedicalRecord { get; set; }
    public Invoices.Invoice? Invoice { get; set; }
    public ICollection<AppointmentStatusHistory> StatusHistories { get; set; } = [];
}
