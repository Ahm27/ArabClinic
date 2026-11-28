using System.Security.Claims;

using ArabClinic.Application.Accounts;
using ArabClinic.Web.Models.Account;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

public sealed class AccountController(
    IAccountService accountService) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!model.AcceptTerms)
        {
            ModelState.AddModelError(nameof(model.AcceptTerms), "You must accept the terms and privacy policy.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new RegisterAccountRequest(
            model.FullName,
            model.Email,
            model.PhoneNumber,
            model.DateOfBirth!.Value,
            model.Gender,
            model.Password);

        var result = await accountService.RegisterAsync(request);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new LoginAccountRequest(
            model.Email,
            model.Password,
            model.RememberMe);

        var result = await accountService.LoginAsync(request);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var profile = await accountService.GetProfileAsync(userId);

        if (profile is null)
        {
            return NotFound();
        }

        return View(profile);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await accountService.LogoutAsync();

        return RedirectToAction("Index", "Home");
    }
}