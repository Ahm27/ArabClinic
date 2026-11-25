namespace ArabClinic.Domain.MedicalRecords;

public class Prescription
{
    public int PrescriptionId { get; set; }
    public int MedicalRecordId { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public MedicalRecord? MedicalRecord { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = [];
}
