using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController(AppDbContext context) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();

            var notifications = await context.Notifications
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

            return Ok(ResponseModel<List<ReturnNotificationDto>>.Ok(notifications));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();

            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound(ResponseModel<string>.Fail("Notification not found."));

            notification.IsRead = true;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Marked as read."));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();

            var notifications = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            notifications.ForEach(n => n.IsRead = true);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "All marked as read."));
        }
    }
}