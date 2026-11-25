using ArabClinic.Application.Notifications;

using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

public sealed class NotificationsController(INotificationService notificationService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var notifications = await notificationService.GetAllAsync(cancellationToken: cancellationToken);
        var unreadCount = await notificationService.GetUnreadCountAsync(cancellationToken: cancellationToken);
        ViewBag.UnreadCount = unreadCount;
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        await notificationService.MarkAsReadAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
