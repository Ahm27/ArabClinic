using ArabClinic.Domain.Common;

namespace ArabClinic.Domain.Patients;

public sealed class Patient : Entity
{
    private Patient()
    {
    }

    public Patient(string fullName, DateOnly dateOfBirth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
        DateOfBirth = dateOfBirth;
    }

    public string FullName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
}