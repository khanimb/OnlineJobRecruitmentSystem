using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.API.Hubs;
using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController(
        AppDbContext context,
        IHubContext<ChatHub> chatHub) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            var senderId = GetUserId();

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content ?? string.Empty,
                IsRead = false
            };

            context.Messages.Add(message);
            await context.SaveChangesAsync();

            await chatHub.Clients.User(dto.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.SenderId,
                    message.Content,
                    message.CreatedAt
                });

            return Ok(ResponseModel<string>.Ok(null!, "Message sent."));
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var userId = GetUserId();

            var messages = await context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ReturnMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Email,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver.Email,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnMessageDto>>.Ok(messages));
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var userId = GetUserId();

            var messages = await context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new ReturnMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Email,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnMessageDto>>.Ok(messages));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            var count = await context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

            return Ok(ResponseModel<int>.Ok(count));
        }
    }
}