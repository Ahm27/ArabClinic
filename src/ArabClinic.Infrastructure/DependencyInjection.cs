using ArabClinic.Application.Patients;
using ArabClinic.Infrastructure.Persistence;
using ArabClinic.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArabClinic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("ArabClinic")
            ?? throw new InvalidOperationException(
                "Connection string 'ArabClinic' is not configured.");

        services.AddDbContext<ArabClinicDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}