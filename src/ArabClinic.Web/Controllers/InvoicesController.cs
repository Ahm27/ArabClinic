using ArabClinic.Application.Invoices;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

public sealed class InvoicesController(IBillingService billingService) : Controller
{
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
