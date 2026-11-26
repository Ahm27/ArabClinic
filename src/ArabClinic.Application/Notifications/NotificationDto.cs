namespace ArabClinic.Application.Notifications;

public sealed record NotificationDto(
    int NotificationId,
    string UserId,
    string UserName,
    string Message,
    bool IsRead,
    DateTime CreatedAt,
    IReadOnlyList<DeliveryLogDto> DeliveryLogs);

public sealed record DeliveryLogDto(
    int DeliveryLogId,
    string Channel,
    string DeliveryStatus,
    string? ProviderMessageId,
    string? ErrorMessage,
    DateTime SentAt);

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetAllAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string? userId = null, CancellationToken cancellationToken = default);
}