namespace ArabClinic.Application.Patients;

public sealed record UpdatePatientRequest(string FullName, DateOnly DateOfBirth);