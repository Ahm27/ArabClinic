using ArabClinic.Application.Appointments;
using ArabClinic.Application.Patients;
using ArabClinic.Web.Models.Patients;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

public sealed class PatientsController(
    IPatientService patientService,
    IAppointmentService appointmentService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var patients = await patientService.GetAllAsync(cancellationToken);
        return View(patients.Select(patient => new PatientListItemViewModel(
            patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender, patient.Address, patient.Allergies)).ToArray());
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        var appointments = await appointmentService.GetAllAsync(patientId: id, cancellationToken: cancellationToken);

        return View(new PatientDetailsViewModel(
            patient.Id,
            patient.FullName,
            patient.DateOfBirth,
            patient.Gender,
            patient.Address,
            patient.Allergies,
            patient.MedicalHistory,
            appointments));
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
                new CreatePatientRequest(
                    model.FullName,
                    model.DateOfBirth!.Value,
                    model.Gender,
                    model.Address,
                    model.Allergies,
                    model.MedicalHistory),
                cancellationToken);

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
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Address = patient.Address,
            Allergies = patient.Allergies,
            MedicalHistory = patient.MedicalHistory
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
                id,
                new UpdatePatientRequest(
                    model.FullName,
                    model.DateOfBirth!.Value,
                    model.Gender,
                    model.Address,
                    model.Allergies,
                    model.MedicalHistory),
                cancellationToken);

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
        return patient is null
            ? NotFound()
            : View(new PatientDetailsViewModel(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender, patient.Address, patient.Allergies, patient.MedicalHistory));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        await patientService.DeleteAsync(id, cancellationToken);
        TempData["PatientMessage"] = "Patient deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}