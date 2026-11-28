using System.Security.Claims;

using ArabClinic.Application.Invoices;
using ArabClinic.Domain.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

[Authorize]
public sealed class InvoicesController(IBillingService billingService) : Controller
{
    [Authorize(Roles = ClinicRoles.Admin)]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        ViewData["CurrentStatus"] = status ?? "All";
        var invoices = await billingService.GetAllAsync(status, cancellationToken);
        return View(invoices);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var invoice = await billingService.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
        {
            return NotFound();
        }
        if (!User.IsInRole(ClinicRoles.Admin) && invoice.PatientId.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

        return View(invoice);
    }

    public async Task<IActionResult> ByAppointment(int appointmentId, CancellationToken cancellationToken)
    {
        var invoice = await billingService.GetByAppointmentIdAsync(appointmentId, cancellationToken);
        if (invoice == null)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ClinicRoles.Admin)]
    public async Task<IActionResult> Pay(RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var (success, errorMessage) = await billingService.RecordPaymentAsync(request, cancellationToken);
        if (!success)
        {
            TempData["PaymentError"] = errorMessage ?? "Payment could not be processed.";
        }
        else
        {
            TempData["PaymentSuccess"] = $"Payment of {request.Amount:C} successfully recorded.";
        }

        return RedirectToAction(nameof(Details), new { id = request.InvoiceId });
    }
}