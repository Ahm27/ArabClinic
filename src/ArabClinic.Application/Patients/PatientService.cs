using ArabClinic.Domain.Patients;

namespace ArabClinic.Application.Patients;

public sealed class PatientService(IPatientRepository repository) : IPatientService
{
    public async Task<IReadOnlyList<PatientDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var patients = await repository.GetAllAsync(cancellationToken);
        return patients.Select(Map).ToArray();
    }

    public async Task<PatientDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var patient = await repository.GetByIdAsync(id, cancellationToken);
        return patient is null ? null : Map(patient);
    }

    public async Task<Guid> CreateAsync(
        CreatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var patient = new Patient(request.FullName, request.DateOfBirth, request.Gender, request.Address, request.Allergies, request.MedicalHistory);
        await repository.AddAsync(patient, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return patient.Id;
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var patient = await repository.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return false;
        }

        patient.UpdateFullDetails(request.FullName, request.DateOfBirth, request.Gender, request.Address, request.Allergies, request.MedicalHistory);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var patient = await repository.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return false;
        }

        repository.Remove(patient);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PatientDto Map(Patient patient) =>
        new(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender, patient.Address, patient.Allergies, patient.MedicalHistory);
}