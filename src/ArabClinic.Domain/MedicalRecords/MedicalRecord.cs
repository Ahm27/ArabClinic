namespace ArabClinic.Domain.MedicalRecords;

public class MedicalRecord
{
    public int MedicalRecordId { get; set; }
    public int AppointmentId { get; set; } // FK UNIQUE
    public string Diagnosis { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? TreatmentNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Appointments.Appointment? Appointment { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = [];
    public ICollection<MedicalDocument> Documents { get; set; } = [];
}
