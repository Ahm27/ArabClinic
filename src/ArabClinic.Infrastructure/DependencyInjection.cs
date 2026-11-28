using ArabClinic.Application.Accounts;
using ArabClinic.Application.Appointments;
using ArabClinic.Application.Doctors;
using ArabClinic.Application.Invoices;
using ArabClinic.Application.MedicalRecords;
using ArabClinic.Application.Notifications;
using ArabClinic.Application.Patients;
using ArabClinic.Application.Specializations;
using ArabClinic.Infrastructure.Persistence;
using ArabClinic.Infrastructure.Persistence.Repositories;
using ArabClinic.Infrastructure.Services;

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
            ?? throw new InvalidOperationException("Connection string 'ArabClinic' is not configured.");

        services.AddDbContext<ArabClinicDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(3);
            });
        });

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAdminDoctorService, AdminDoctorService>();
        services.AddScoped<ISpecializationService, SpecializationService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IMedicalRecordService, MedicalRecordService>();
        services.AddScoped<IMedicalVisitReadService, MedicalVisitReadService>();
        services.AddScoped<IMedicalVisitWriteService, MedicalVisitWriteService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}