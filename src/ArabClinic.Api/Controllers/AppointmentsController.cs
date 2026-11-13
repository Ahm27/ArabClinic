using ArabClinic.Api.Models;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Api.Controllers;

public sealed class AppointmentsController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Create() => View(new AppointmentViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["AppointmentSubmitted"] = true;
        return RedirectToAction(nameof(Index));
    }
}