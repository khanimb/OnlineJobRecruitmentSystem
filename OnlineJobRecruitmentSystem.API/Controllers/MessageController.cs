using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineJobRecruitmentSystem.API.Hubs;
using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController(
        IMessageService messageService,
        IHubContext<ChatHub> chatHub,
        INotificationService notificationService
        ) : BaseApiController
    {
        

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            var senderId = CurrentUserId;
            var message = await messageService.SaveMessageAsync(senderId, dto);

            await chatHub.Clients.User(dto.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.SenderId,
                    message.Content,
                    message.CreatedAt
                });

            await notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = dto.ReceiverId,
                Title = "New Message",
                Message = "You have received a new message.",
                Type = "Message"
            });

            return Ok(ResponseModel<string>.Ok(null!, "Message sent."));
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var messages = await messageService.GetConversationAsync(CurrentUserId, otherUserId);
            return Ok(ResponseModel<List<ReturnMessageDto>>.Ok(messages));
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var messages = await messageService.GetInboxAsync(CurrentUserId);
            return Ok(ResponseModel<List<ReturnMessageDto>>.Ok(messages));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await messageService.GetUnreadCountAsync(CurrentUserId);
            return Ok(ResponseModel<int>.Ok(count));
        }
    }
}