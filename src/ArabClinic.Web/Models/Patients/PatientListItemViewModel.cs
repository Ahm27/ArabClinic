namespace ArabClinic.Web.Models.Patients;

public sealed record PatientListItemViewModel(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    string Gender = "NotSpecified",
    string? Address = null,
    string? Allergies = null);
