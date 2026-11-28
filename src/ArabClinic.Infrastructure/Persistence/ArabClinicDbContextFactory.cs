using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArabClinic.Infrastructure.Persistence;

public sealed class ArabClinicDbContextFactory : IDesignTimeDbContextFactory<ArabClinicDbContext>
{
    public ArabClinicDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ArabClinic")
            ?? "Server=localhost,1433;Database=MediCare;Integrated Security=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<ArabClinicDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ArabClinicDbContext(options);
    }
}