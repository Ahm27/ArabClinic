using ArabClinic.Application.Accounts;
using ArabClinic.Domain.Users;

using Microsoft.AspNetCore.Identity;

namespace ArabClinic.Infrastructure.Services;

public sealed class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IAccountService
{
    private static readonly string[] AccountLockedErrors =
    {
        "Your account is locked. Please try again later."
    };

    private static readonly string[] InvalidLoginErrors =
    {
        "Invalid login attempt."
    };

    public async Task<AccountResult> RegisterAsync(
        RegisterAccountRequest request)
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
            var errors = result.Errors
                .Select(error => error.Description)
                .ToArray();

            return new AccountResult(false, errors);
        }

        await signInManager.SignInAsync(user, isPersistent: false);

        return new AccountResult(true, Array.Empty<string>());
    }

    public async Task<AccountResult> LoginAsync(
        LoginAccountRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Email.Trim(),
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return new AccountResult(true, Array.Empty<string>());
        }

        if (result.IsLockedOut)
        {
            return new AccountResult(
                false,
                AccountLockedErrors,
                true);
        }

        return new AccountResult(
            false,
            InvalidLoginErrors);
    }

    public async Task<AccountProfile?> GetProfileAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return null;
        }

        return new AccountProfile(
            user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber);
    }
    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }
}