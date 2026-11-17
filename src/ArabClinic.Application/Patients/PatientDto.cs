namespace ArabClinic.Application.Patients;

public sealed record PatientDto(Guid Id, string FullName, DateOnly DateOfBirth);