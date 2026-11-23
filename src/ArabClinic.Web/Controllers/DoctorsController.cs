using ArabClinic.Application.Doctors;
using ArabClinic.Application.Specializations;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

public sealed class DoctorsController(
    IDoctorService doctorService,
    ISpecializationService specializationService) : Controller
{
    public async Task<IActionResult> Index(int? specializationId, CancellationToken cancellationToken)
    {
        ViewData["SelectedSpecializationId"] = specializationId;
        var specializations = await specializationService.GetAllAsync(cancellationToken);
        var doctors = await doctorService.GetAllAsync(specializationId, cancellationToken);

        ViewBag.Specializations = specializations;
        return View(doctors);
    }

    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var doctor = await doctorService.GetByIdAsync(id, cancellationToken);
        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }
}
