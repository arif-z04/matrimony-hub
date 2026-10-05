using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, string? relatedEntityId = null);
    Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, int take = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task<ServiceResult> MarkAsReadAsync(int userId, int notificationId);
    Task<ServiceResult> MarkAllAsReadAsync(int userId);
}
