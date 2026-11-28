namespace ArabClinic.Domain.MedicalRecords;

public sealed class MedicalVisit
{
    public int MedicalVisitId { get; set; }
    public Guid PatientId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? TreatmentNotes { get; set; }
    public string? FollowUpNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Patients.Patient? Patient { get; set; }
    public Doctors.Doctor? Doctor { get; set; }
    public Appointments.Appointment? Appointment { get; set; }
}