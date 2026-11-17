using ArabClinic.Application.Patients;
using ArabClinic.Domain.Patients;

namespace ArabClinic.UnitTests.Application.Patients;

public sealed class PatientServiceTests
{
    [Fact]
    public async Task CreateAddsPatientAndReturnsUniqueId()
    {
        var repository = new FakePatientRepository();
        var service = new PatientService(repository);

        var firstId = await service.CreateAsync(
            new CreatePatientRequest("Ahmed Ali", new DateOnly(1990, 1, 1)));
        var secondId = await service.CreateAsync(
            new CreatePatientRequest("Mariam Adel", new DateOnly(1992, 2, 2)));

        Assert.NotEqual(firstId, secondId);
        Assert.Equal(2, repository.Patients.Count);
        Assert.Equal(2, repository.SaveCount);
    }

    [Fact]
    public async Task GetByIdReturnsExistingPatient()
    {
        var patient = new Patient("Ahmed Ali", new DateOnly(1990, 1, 1));
        var service = new PatientService(new FakePatientRepository(patient));

        var result = await service.GetByIdAsync(patient.Id);

        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.Id);
        Assert.Equal("Ahmed Ali", result.FullName);
    }

    [Fact]
    public async Task GetByIdReturnsNullForNonexistentPatient()
    {
        var service = new PatientService(new FakePatientRepository());

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateChangesExistingPatient()
    {
        var patient = new Patient("Ahmed Ali", new DateOnly(1990, 1, 1));
        var repository = new FakePatientRepository(patient);
        var service = new PatientService(repository);

        var updated = await service.UpdateAsync(
            patient.Id,
            new UpdatePatientRequest("Ahmed Hassan", new DateOnly(1991, 2, 2)));

        Assert.True(updated);
        Assert.Equal("Ahmed Hassan", patient.FullName);
        Assert.Equal(new DateOnly(1991, 2, 2), patient.DateOfBirth);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task UpdateReturnsFalseForNonexistentPatient()
    {
        var repository = new FakePatientRepository();
        var service = new PatientService(repository);

        var updated = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdatePatientRequest("Ahmed Hassan", new DateOnly(1991, 2, 2)));

        Assert.False(updated);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task DeleteRemovesExistingPatient()
    {
        var patient = new Patient("Ahmed Ali", new DateOnly(1990, 1, 1));
        var repository = new FakePatientRepository(patient);
        var service = new PatientService(repository);

        var deleted = await service.DeleteAsync(patient.Id);

        Assert.True(deleted);
        Assert.Empty(repository.Patients);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task DeleteReturnsFalseForNonexistentPatient()
    {
        var repository = new FakePatientRepository();
        var service = new PatientService(repository);

        var deleted = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task CreateRejectsInvalidName()
    {
        var repository = new FakePatientRepository();
        var service = new PatientService(repository);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            new CreatePatientRequest(" ", new DateOnly(1990, 1, 1))));

        Assert.Empty(repository.Patients);
        Assert.Equal(0, repository.SaveCount);
    }

    private sealed class FakePatientRepository(params Patient[] patients) : IPatientRepository
    {
        public List<Patient> Patients { get; } = [.. patients];
        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Patient>>(Patients);

        public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Patients.SingleOrDefault(patient => patient.Id == id));

        public Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            Patients.Add(patient);
            return Task.CompletedTask;
        }

        public void Remove(Patient patient) => Patients.Remove(patient);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}