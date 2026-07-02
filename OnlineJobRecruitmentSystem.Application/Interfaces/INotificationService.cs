using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(CreateNotificationDto dto);
        Task<List<ReturnNotificationDto>> GetUserNotificationsAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}