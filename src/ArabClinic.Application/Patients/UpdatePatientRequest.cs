namespace ArabClinic.Application.Patients;

public sealed record UpdatePatientRequest(
    string FullName,
    DateOnly DateOfBirth,
    string Gender = "NotSpecified",
    string? Address = null,
    string? Allergies = null,
    string? MedicalHistory = null);