using ArabClinic.Domain.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArabClinic.Infrastructure.Persistence;

public static class IdentityInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { ClinicRoles.Patient, ClinicRoles.Doctor, ClinicRoles.Admin })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded) throw new InvalidOperationException($"Could not create role '{role}'.");
            }
        }

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                FullName = configuration["AdminSeed:FullName"] ?? "Clinic Administrator",
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        if (!await userManager.IsInRoleAsync(admin, ClinicRoles.Admin))
            await userManager.AddToRoleAsync(admin, ClinicRoles.Admin);
    }
}