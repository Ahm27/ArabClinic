using ArabClinic.Application.Accounts;
using ArabClinic.Domain.Patients;
using ArabClinic.Domain.Users;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;

namespace ArabClinic.Infrastructure.Services;

public sealed class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ArabClinicDbContext dbContext) : IAccountService
{
    public async Task<AccountResult> RegisterAsync(RegisterAccountRequest request)
    {
        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            UserName = email,
            PhoneNumber = request.PhoneNumber?.Trim()
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AccountResult(false, result.Errors.Select(error => error.Description).ToArray());
        }

        var roleResult = await userManager.AddToRoleAsync(user, ClinicRoles.Patient);
        if (!roleResult.Succeeded || !Guid.TryParse(user.Id, out var patientId))
        {
            await userManager.DeleteAsync(user);
            var errors = roleResult.Succeeded
                ? new[] { "The generated account identifier is invalid." }
                : roleResult.Errors.Select(error => error.Description).ToArray();
            return new AccountResult(false, errors);
        }

        try
        {
            await dbContext.Patients.AddAsync(new Patient(patientId, request.FullName, request.DateOfBirth, request.Gender)
            {
                UserId = user.Id
            });
            await dbContext.SaveChangesAsync();
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return new AccountResult(true, []);
    }

    public async Task<AccountResult> LoginAsync(LoginAccountRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Email.Trim(), request.Password, request.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return new AccountResult(true, []);
        }
        return result.IsLockedOut
            ? new AccountResult(false, ["Your account is locked. Please try again later."], true)
            : new AccountResult(false, ["Invalid email or password."]);
    }

    public async Task LogoutAsync() => await signInManager.SignOutAsync();

    public async Task<AccountProfile?> GetProfileAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }
        var roles = await userManager.GetRolesAsync(user);
        return new AccountProfile(user.FullName, user.Email ?? string.Empty, user.PhoneNumber, roles.ToArray());
    }
}