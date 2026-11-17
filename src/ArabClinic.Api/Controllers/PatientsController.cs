using ArabClinic.Api.Models.Patients;
using ArabClinic.Application.Patients;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Api.Controllers;

public sealed class PatientsController(IPatientService patientService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var patients = await patientService.GetAllAsync(cancellationToken);
        return View(patients.Select(patient => new PatientListItemViewModel(
            patient.Id, patient.FullName, patient.DateOfBirth)).ToArray());
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patientService.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : View(ToDetailsViewModel(patient));
    }

    public IActionResult Create() => View(new PatientFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var id = await patientService.CreateAsync(
                new CreatePatientRequest(model.FullName, model.DateOfBirth!.Value), cancellationToken);
            TempData["PatientMessage"] = "Patient created successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(nameof(model.FullName), exception.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return View(new PatientFormViewModel
        {
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PatientFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated = await patientService.UpdateAsync(
                id, new UpdatePatientRequest(model.FullName, model.DateOfBirth!.Value), cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            TempData["PatientMessage"] = "Patient updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(nameof(model.FullName), exception.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patientService.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : View(ToDetailsViewModel(patient));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        if (!await patientService.DeleteAsync(id, cancellationToken))
        {
            return NotFound();
        }

        TempData["PatientMessage"] = "Patient deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private static PatientDetailsViewModel ToDetailsViewModel(PatientDto patient) =>
        new(patient.Id, patient.FullName, patient.DateOfBirth);
}