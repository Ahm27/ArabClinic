namespace ArabClinic.Api.Models.Patients;

public sealed record PatientListItemViewModel(Guid Id, string FullName, DateOnly DateOfBirth);