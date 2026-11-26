using ArabClinic.Application.Doctors;
using ArabClinic.Application.Specializations;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

public sealed class DepartmentsController(
    ISpecializationService specializationService,
    IDoctorService doctorService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var specializations = await specializationService.GetAllAsync(cancellationToken);
        return View(specializations);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var specialization = await specializationService.GetByIdAsync(id, cancellationToken);
        if (specialization == null)
        {
            return NotFound();
        }

        var doctors = await doctorService.GetAllAsync(id, cancellationToken);
        ViewBag.Doctors = doctors;

        return View(specialization);
    }
}