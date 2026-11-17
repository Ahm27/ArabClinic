using ArabClinic.Domain.Patients;
using ArabClinic.Infrastructure.Persistence;
using ArabClinic.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.UnitTests.Infrastructure.Patients;

public sealed class PatientRepositoryTests
{
    [Fact]
    public async Task RepositoryPersistsReadsUpdatesAndDeletesPatient()
    {
        var options = new DbContextOptionsBuilder<ArabClinicDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new ArabClinicDbContext(options);
        var repository = new PatientRepository(dbContext);
        var patient = new Patient("Ahmed Ali", new DateOnly(1990, 1, 1));

        await repository.AddAsync(patient);
        await repository.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var persisted = await repository.GetByIdAsync(patient.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Ahmed Ali", persisted.FullName);

        persisted.UpdateDetails("Ahmed Hassan", new DateOnly(1991, 2, 2));
        await repository.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var updated = await repository.GetByIdAsync(patient.Id);
        Assert.NotNull(updated);
        Assert.Equal("Ahmed Hassan", updated.FullName);
        Assert.Equal(new DateOnly(1991, 2, 2), updated.DateOfBirth);

        repository.Remove(updated);
        await repository.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        Assert.Null(await repository.GetByIdAsync(patient.Id));
    }
}