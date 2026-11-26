using ArabClinic.Application.Notifications;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class NotificationService(ArabClinicDbContext dbContext) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetAllAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Notifications
            .AsNoTracking()
            .Include(n => n.User)
            .Include(n => n.DeliveryLogs)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(n => n.UserId == userId);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return notifications.Select(n => new NotificationDto(
            n.NotificationId,
            n.UserId,
            n.User?.FullName ?? "User",
            n.Message,
            n.IsRead,
            n.CreatedAt,
            n.DeliveryLogs.Select(l => new DeliveryLogDto(
                l.DeliveryLogId,
                l.Channel,
                l.DeliveryStatus,
                l.ProviderMessageId,
                l.ErrorMessage,
                l.SentAt
            )).ToList()
        )).ToList();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var notif = await dbContext.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken);
        if (notif == null)
        {
            return false;
        }

        notif.IsRead = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> GetUnreadCountAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Notifications.Where(n => !n.IsRead);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(n => n.UserId == userId);
        }
        return await query.CountAsync(cancellationToken);
    }
}