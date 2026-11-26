using ArabClinic.Application.Appointments;
using ArabClinic.Application.Doctors;
using ArabClinic.Application.MedicalRecords;
using ArabClinic.Application.Patients;
using ArabClinic.Web.Models.MedicalRecords;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArabClinic.Web.Controllers;

public sealed class MedicalRecordsController(
    IMedicalRecordService medicalRecordService,
    IAppointmentService appointmentService,
    IMedicalVisitReadService medicalVisitReadService,
    IMedicalVisitWriteService medicalVisitWriteService,
    IPatientService patientService,
    IDoctorService doctorService) : Controller
{
    public async Task<IActionResult> VisitDetails(int id, CancellationToken cancellationToken)
    {
        var visit = await medicalVisitReadService.GetByIdAsync(id, cancellationToken);
        return visit is null ? NotFound() : View(visit);
    }

    public async Task<IActionResult> CreateVisit(Guid? patientId, CancellationToken cancellationToken)
    {
        var model = new CreateMedicalVisitViewModel
        {
            PatientId = patientId ?? Guid.Empty,
            VisitDate = DateTime.Now
        };
        await PopulateVisitOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVisit(CreateMedicalVisitViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateVisitOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var result = await medicalVisitWriteService.CreateAsync(
            new CreateMedicalVisitRequest(model.PatientId, model.DoctorId, model.AppointmentId, model.VisitDate, model.Summary),
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not save visit.");
            await PopulateVisitOptionsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["RecordSuccess"] = "Clinical visit recorded successfully.";
        return RedirectToAction(nameof(VisitDetails), new { id = result.MedicalVisitId });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var record = await medicalRecordService.GetByIdAsync(id, cancellationToken);
        if (record == null)
        {
            return NotFound();
        }

        return View(record);
    }

    public async Task<IActionResult> ByAppointment(int appointmentId, CancellationToken cancellationToken)
    {
        var record = await medicalRecordService.GetByAppointmentIdAsync(appointmentId, cancellationToken);
        if (record == null)
        {
            return RedirectToAction(nameof(Create), new { appointmentId });
        }

        return View(nameof(Details), record);
    }

    public async Task<IActionResult> Create(int appointmentId, CancellationToken cancellationToken)
    {
        var app = await appointmentService.GetByIdAsync(appointmentId, cancellationToken);
        if (app == null)
        {
            return NotFound();
        }

        ViewBag.Appointment = app;
        return View(new CreateMedicalRecordViewModel { AppointmentId = appointmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMedicalRecordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var app = await appointmentService.GetByIdAsync(model.AppointmentId, cancellationToken);
            ViewBag.Appointment = app;
            return View(model);
        }

        var items = new List<CreatePrescriptionItemRequest>();
        if (!string.IsNullOrWhiteSpace(model.MedicationName1))
        {
            items.Add(new CreatePrescriptionItemRequest(
                model.MedicationName1,
                model.Dosage1 ?? "1 tablet",
                model.Frequency1 ?? "Daily",
                model.Duration1 ?? "30 days"
            ));
        }
        if (!string.IsNullOrWhiteSpace(model.MedicationName2))
        {
            items.Add(new CreatePrescriptionItemRequest(
                model.MedicationName2,
                model.Dosage2 ?? "1 tablet",
                model.Frequency2 ?? "Daily",
                model.Duration2 ?? "30 days"
            ));
        }

        var request = new CreateMedicalRecordRequest(
            model.AppointmentId,
            model.Diagnosis,
            model.Symptoms,
            model.TreatmentNotes,
            items
        );

        var (success, errorMessage, medicalRecordId) = await medicalRecordService.CreateAsync(request, cancellationToken);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Could not create medical record.");
            var app = await appointmentService.GetByIdAsync(model.AppointmentId, cancellationToken);
            ViewBag.Appointment = app;
            return View(model);
        }

        TempData["RecordSuccess"] = "Medical record and prescription created successfully.";
        return RedirectToAction(nameof(Details), new { id = medicalRecordId });
    }

    private async Task PopulateVisitOptionsAsync(CreateMedicalVisitViewModel model, CancellationToken cancellationToken)
    {
        var patients = await patientService.GetAllAsync(cancellationToken);
        var doctors = await doctorService.GetAllAsync(cancellationToken: cancellationToken);
        model.PatientOptions = patients.Select(patient => new SelectListItem(patient.FullName, patient.Id.ToString())).ToList();
        model.DoctorOptions = doctors.Select(doctor => new SelectListItem(doctor.FullName, doctor.DoctorId)).ToList();
    }
}