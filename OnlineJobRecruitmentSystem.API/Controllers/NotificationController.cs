using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController(INotificationService notificationService) : BaseApiController
    {
        

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await notificationService.GetUserNotificationsAsync(CurrentUserId);
            return Ok(ResponseModel<List<ReturnNotificationDto>>.Ok(notifications));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var marked = await notificationService.MarkAsReadAsync(id, CurrentUserId);
            if (!marked)
                return NotFound(ResponseModel<string>.Fail("Notification not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Marked as read."));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await notificationService.MarkAllAsReadAsync(CurrentUserId);
            return Ok(ResponseModel<string>.Ok(null!, "All marked as read."));
        }
    }
}