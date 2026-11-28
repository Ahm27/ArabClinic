using ArabClinic.Domain.Appointments;
using ArabClinic.Domain.Common;
using ArabClinic.Domain.Users;

namespace ArabClinic.Domain.Patients;

public sealed class Patient : Entity
{
    public string Gender { get; set; } = "NotSpecified";
    public string? Address { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];

    public Patient()
    {
    }

    public Patient(string fullName, DateOnly dateOfBirth)
    {
        UpdateDetails(fullName, dateOfBirth);
    }

    public Patient(Guid id, string fullName, DateOnly dateOfBirth, string gender)
        : this(fullName, dateOfBirth, gender)
    {
        Id = id;
    }

    public Patient(string fullName, DateOnly dateOfBirth, string gender, string? address = null, string? allergies = null, string? medicalHistory = null)
    {
        UpdateDetails(fullName, dateOfBirth);
        Gender = string.IsNullOrWhiteSpace(gender) ? "NotSpecified" : gender.Trim();
        Address = address?.Trim();
        Allergies = allergies?.Trim();
        MedicalHistory = medicalHistory?.Trim();
    }

    public string FullName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }

    public void UpdateDetails(string fullName, DateOnly dateOfBirth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
        DateOfBirth = dateOfBirth;
    }

    public void UpdateFullDetails(string fullName, DateOnly dateOfBirth, string gender, string? address, string? allergies, string? medicalHistory)
    {
        UpdateDetails(fullName, dateOfBirth);
        Gender = string.IsNullOrWhiteSpace(gender) ? "NotSpecified" : gender.Trim();
        Address = address?.Trim();
        Allergies = allergies?.Trim();
        MedicalHistory = medicalHistory?.Trim();
    }

    public void Archive()
    {
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
    }
}