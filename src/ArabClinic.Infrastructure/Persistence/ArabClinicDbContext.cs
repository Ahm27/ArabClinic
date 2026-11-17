using ArabClinic.Domain.Patients;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Persistence;

public sealed class ArabClinicDbContext(DbContextOptions<ArabClinicDbContext> options)
    : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArabClinicDbContext).Assembly);
    }
}