using System.Diagnostics;

using ArabClinic.Application.Doctors;
using ArabClinic.Application.Specializations;
using ArabClinic.Web.Models;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

public sealed class HomeController(
    IDoctorService doctorService,
    ISpecializationService specializationService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var specializations = await specializationService.GetAllAsync(cancellationToken);
        var doctors = await doctorService.GetAllAsync(cancellationToken: cancellationToken);

        ViewBag.Specializations = specializations;
        ViewBag.Doctors = doctors.Take(4).ToList();

        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}