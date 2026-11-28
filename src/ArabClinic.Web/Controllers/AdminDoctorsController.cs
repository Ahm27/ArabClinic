using System.Globalization;

using ArabClinic.Application.Doctors;
using ArabClinic.Application.Specializations;
using ArabClinic.Domain.Users;
using ArabClinic.Web.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArabClinic.Web.Controllers;

[Authorize(Roles = ClinicRoles.Admin)]
public sealed class AdminDoctorsController(IAdminDoctorService admin, ISpecializationService specializations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await admin.GetAllAsync(cancellationToken));
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new DoctorAdminViewModel(); await Populate(model, cancellationToken); return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorAdminViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await admin.CreateAsync(new CreateDoctorRequest(model.FullName, model.Email, model.PhoneNumber, model.Password,
                model.SpecializationId, model.ConsultationFee, model.Location, model.SlotDurationMinutes), cancellationToken);
            if (result.Success) return RedirectToAction(nameof(Index));
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
        }
        await Populate(model, cancellationToken); return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(string id, bool active, CancellationToken cancellationToken)
    {
        if (!await admin.SetActiveAsync(id, active, cancellationToken)) return NotFound();
        return RedirectToAction(nameof(Index));
    }
    private async Task Populate(DoctorAdminViewModel model, CancellationToken token) => model.Specializations =
        (await specializations.GetAllAsync(token)).Select(item => new SelectListItem(
            item.Name, item.SpecializationId.ToString(CultureInfo.InvariantCulture))).ToList();
}