using ArabClinic.Application.Appointments;

namespace ArabClinic.Web.Models.Patients;

public sealed record PatientDetailsViewModel(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    string Gender = "NotSpecified",
    string? Address = null,
    string? Allergies = null,
    string? MedicalHistory = null,
    IReadOnlyList<AppointmentDto>? Appointments = null);