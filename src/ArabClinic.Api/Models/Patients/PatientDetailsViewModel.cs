namespace ArabClinic.Api.Models.Patients;

public sealed record PatientDetailsViewModel(Guid Id, string FullName, DateOnly DateOfBirth);