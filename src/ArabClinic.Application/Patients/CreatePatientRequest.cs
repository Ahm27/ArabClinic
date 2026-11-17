namespace ArabClinic.Application.Patients;

public sealed record CreatePatientRequest(string FullName, DateOnly DateOfBirth);