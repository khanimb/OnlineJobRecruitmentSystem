using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await notificationService.GetUserNotificationsAsync(GetUserId());
            return Ok(ResponseModel<List<ReturnNotificationDto>>.Ok(notifications));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var marked = await notificationService.MarkAsReadAsync(id, GetUserId());
            if (!marked)
                return NotFound(ResponseModel<string>.Fail("Notification not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Marked as read."));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await notificationService.MarkAllAsReadAsync(GetUserId());
            return Ok(ResponseModel<string>.Ok(null!, "All marked as read."));
        }
    }
}