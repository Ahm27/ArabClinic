namespace ArabClinic.Application.Patients;

public sealed record PatientDto(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    string Gender = "NotSpecified",
    string? Address = null,
    string? Allergies = null,
    string? MedicalHistory = null);
