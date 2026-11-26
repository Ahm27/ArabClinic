namespace ArabClinic.Domain.MedicalRecords;

public class MedicalDocument
{
    public int DocumentId { get; set; }
    public int MedicalRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public MedicalRecord? MedicalRecord { get; set; }
}