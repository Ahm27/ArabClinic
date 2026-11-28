using ArabClinic.Domain.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["DashboardRole"] = User.IsInRole(ClinicRoles.Admin)
            ? ClinicRoles.Admin
            : User.IsInRole(ClinicRoles.Doctor) ? ClinicRoles.Doctor : ClinicRoles.Patient;
        return View();
    }
}