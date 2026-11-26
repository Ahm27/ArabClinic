using ArabClinic.Application.Appointments;
using ArabClinic.Application.Doctors;
using ArabClinic.Application.Specializations;
using ArabClinic.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArabClinic.Web.Controllers;

public sealed class AppointmentsController(
    IAppointmentService appointmentService,
    IDoctorService doctorService,
    ISpecializationService specializationService) : Controller
{
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        ViewData["CurrentStatus"] = status ?? "All";
        var appointments = await appointmentService.GetAllAsync(status, cancellationToken: cancellationToken);
        return View(appointments);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
        {
            return NotFound();
        }

        return View(appointment);
    }

    public async Task<IActionResult> Create(string? doctorId, CancellationToken cancellationToken)
    {
        var model = new AppointmentViewModel
        {
            DoctorId = doctorId ?? string.Empty,
            PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            PreferredTime = new TimeSpan(10, 0, 0)
        };

        await PopulateDropdownsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, cancellationToken);
            return View(model);
        }

        var timeOnly = TimeOnly.FromTimeSpan(model.PreferredTime!.Value);
        var startDateTime = model.PreferredDate!.Value.ToDateTime(timeOnly);
        var endDateTime = startDateTime.AddMinutes(30);

        var request = new BookAppointmentRequest(
            model.PatientId,
            model.FullName,
            model.Email,
            model.Phone,
            model.DoctorId,
            startDateTime,
            endDateTime,
            model.Notes
        );

        var (success, errorMessage, appointmentId) = await appointmentService.BookAppointmentAsync(request, cancellationToken);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Could not schedule appointment.");
            await PopulateDropdownsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["AppointmentSuccess"] = "Your appointment has been successfully scheduled! You can view details below.";
        return RedirectToAction(nameof(Details), new { id = appointmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var success = await appointmentService.CancelAsync(id, "Patient/Receptionist", cancellationToken);
        if (!success)
        {
            TempData["AppointmentError"] = "Unable to cancel appointment.";
        }
        else
        {
            TempData["AppointmentSuccess"] = "Appointment has been cancelled.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var success = await appointmentService.UpdateStatusAsync(id, "Completed", "Clinic Staff", cancellationToken);
        if (!success)
        {
            TempData["AppointmentError"] = "Unable to update status.";
        }
        else
        {
            TempData["AppointmentSuccess"] = "Appointment marked as completed.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateDropdownsAsync(AppointmentViewModel model, CancellationToken cancellationToken)
    {
        var doctors = await doctorService.GetAllAsync(cancellationToken: cancellationToken);
        var specializations = await specializationService.GetAllAsync(cancellationToken);

        model.DoctorOptions = doctors.Select(d => new SelectListItem
        {
            Value = d.DoctorId,
            Text = $"{d.FullName} - {d.SpecializationName} ({d.ConsultationFee:C0})",
            Selected = d.DoctorId == model.DoctorId
        }).ToList();

        model.DepartmentOptions = specializations.Select(s => new SelectListItem
        {
            Value = s.Name,
            Text = s.Name,
            Selected = s.Name == model.Department
        }).ToList();
    }
}