using ArabClinic.Application.Patients;
using ArabClinic.Domain.Patients;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Persistence.Repositories;

public sealed class PatientRepository(ArabClinicDbContext dbContext) : IPatientRepository
{
    public async Task<IReadOnlyList<Patient>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Patients
            .AsNoTracking()
            .OrderBy(patient => patient.FullName)
            .ToListAsync(cancellationToken);

    public Task<Patient?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Patients.SingleOrDefaultAsync(
            patient => patient.Id == id,
            cancellationToken);

    public async Task AddAsync(
        Patient patient,
        CancellationToken cancellationToken = default) =>
        await dbContext.Patients.AddAsync(patient, cancellationToken);

    public void Remove(Patient patient) => dbContext.Patients.Remove(patient);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}