namespace ArabClinic.Web.Models.MedicalRecords;

public sealed class CreateMedicalRecordViewModel
{
    public int AppointmentId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? TreatmentNotes { get; set; }

    // Prescription Item 1
    public string? MedicationName1 { get; set; }
    public string? Dosage1 { get; set; }
    public string? Frequency1 { get; set; }
    public string? Duration1 { get; set; }

    // Prescription Item 2
    public string? MedicationName2 { get; set; }
    public string? Dosage2 { get; set; }
    public string? Frequency2 { get; set; }
    public string? Duration2 { get; set; }
}