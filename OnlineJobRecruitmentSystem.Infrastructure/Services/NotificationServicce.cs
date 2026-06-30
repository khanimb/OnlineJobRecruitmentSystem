using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title ?? string.Empty,
                Message = dto.Message ?? string.Empty,
                Type = dto.Type ?? string.Empty
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<ReturnNotificationDto>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new ReturnNotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}