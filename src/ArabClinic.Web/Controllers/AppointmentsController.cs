using System.Security.Claims;

using ArabClinic.Application.Appointments;
using ArabClinic.Application.Doctors;
using ArabClinic.Application.Specializations;
using ArabClinic.Domain.Users;
using ArabClinic.Web.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArabClinic.Web.Controllers;

[Authorize]
public sealed class AppointmentsController(
    IAppointmentService appointmentService,
    IAvailabilityService availabilityService,
    IDoctorService doctorService,
    ISpecializationService specializationService) : Controller
{
    [HttpGet]
    [Authorize(Roles = ClinicRoles.Patient)]
    public async Task<IActionResult> AvailableSlots(string doctorId, DateOnly date, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(doctorId)) return Json(Array.Empty<AvailableSlotDto>());
        return Json(await availabilityService.GetAvailableSlotsAsync(doctorId, date, cancellationToken));
    }

    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        ViewData["CurrentStatus"] = status ?? "All";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? patientId = User.IsInRole(ClinicRoles.Patient) && Guid.TryParse(userId, out var parsed) ? parsed : null;
        var doctorId = User.IsInRole(ClinicRoles.Doctor) ? userId : null;
        var appointments = await appointmentService.GetAllAsync(status, doctorId, patientId, cancellationToken);
        return View(appointments);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ownsAppointment = User.IsInRole(ClinicRoles.Admin)
            || (User.IsInRole(ClinicRoles.Doctor) && appointment.DoctorId == userId)
            || (User.IsInRole(ClinicRoles.Patient) && appointment.PatientId.ToString() == userId);
        if (!ownsAppointment) return Forbid();

        return View(appointment);
    }

    [Authorize(Roles = ClinicRoles.Patient)]
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
    [Authorize(Roles = ClinicRoles.Patient)]
    public async Task<IActionResult> Create(AppointmentViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, cancellationToken);
            return View(model);
        }

        var timeOnly = TimeOnly.FromTimeSpan(model.PreferredTime!.Value);
        var localStart = DateTime.SpecifyKind(model.PreferredDate!.Value.ToDateTime(timeOnly), DateTimeKind.Unspecified);
        var cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        var startDateTime = TimeZoneInfo.ConvertTimeToUtc(localStart, cairo);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var patientId)) return Challenge();

        var request = new BookAppointmentRequest(
            patientId,
            model.FullName,
            model.Email,
            model.Phone,
            model.DoctorId,
            startDateTime,
            null,
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
    [Authorize(Roles = ClinicRoles.Patient + "," + ClinicRoles.Admin)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment is null) return NotFound();
        if (User.IsInRole(ClinicRoles.Patient) && appointment.PatientId.ToString() != userId) return Forbid();
        if (User.IsInRole(ClinicRoles.Patient) && appointment.StartDateTime <= DateTime.UtcNow.AddHours(24))
        {
            TempData["AppointmentError"] = "Appointments can only be cancelled at least 24 hours in advance.";
            return RedirectToAction(nameof(Details), new { id });
        }
        var success = await appointmentService.CancelAsync(id, userId, cancellationToken);
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
    [Authorize(Roles = ClinicRoles.Doctor + "," + ClinicRoles.Admin)]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment is null) return NotFound();
        if (User.IsInRole(ClinicRoles.Doctor) && appointment.DoctorId != userId) return Forbid();
        var success = await appointmentService.UpdateStatusAsync(id, "Completed", userId, cancellationToken);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ClinicRoles.Doctor + "," + ClinicRoles.Admin)]
    public async Task<IActionResult> ChangeStatus(int id, string status, CancellationToken cancellationToken)
    {
        if (status is not ("Confirmed" or "Rejected")) return BadRequest();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment is null) return NotFound();
        if (User.IsInRole(ClinicRoles.Doctor) && appointment.DoctorId != userId) return Forbid();
        var success = await appointmentService.UpdateStatusAsync(id, status, userId, cancellationToken);
        TempData[success ? "AppointmentSuccess" : "AppointmentError"] = success
            ? $"Appointment {status.ToLowerInvariant()}." : "That status change is not allowed.";
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