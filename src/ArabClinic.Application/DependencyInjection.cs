using ArabClinic.Application.Patients;

using Microsoft.Extensions.DependencyInjection;

namespace ArabClinic.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPatientService, PatientService>();
        return services;
    }
}