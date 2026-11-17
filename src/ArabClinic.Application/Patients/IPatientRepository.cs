using ArabClinic.Domain.Patients;

namespace ArabClinic.Application.Patients;

public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    void Remove(Patient patient);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}