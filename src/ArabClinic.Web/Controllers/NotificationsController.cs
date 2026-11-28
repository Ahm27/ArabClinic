using System.Security.Claims;

using ArabClinic.Application.Notifications;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Web.Controllers;

[Authorize]
public sealed class NotificationsController(INotificationService notificationService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await notificationService.GetAllAsync(userId, cancellationToken);
        var unreadCount = await notificationService.GetUnreadCountAsync(userId, cancellationToken);
        ViewBag.UnreadCount = unreadCount;
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notification = (await notificationService.GetAllAsync(userId, cancellationToken)).FirstOrDefault(item => item.NotificationId == id);
        if (notification is null) return NotFound();
        await notificationService.MarkAsReadAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}