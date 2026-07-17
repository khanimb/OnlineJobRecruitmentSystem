using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly IMessageService _messageService;

        public ChatHub(AppDbContext context, IMessageService messageService)
        {
            _context = context;
            _messageService = messageService;
        }

        public async Task SendMessage(int receiverId, string content)
        {
            var senderIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderIdClaim == null) return;

            int senderId = int.Parse(senderIdClaim);

            if (receiverId == senderId) return;

            var message = await _messageService.SaveMessageAsync(senderId, new SendMessageDto
            {
                ReceiverId = receiverId,
                Content = content
            });

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.SenderId,
                message.Content,
                message.CreatedAt
            });
        }

        public async Task MarkAsRead(int senderId)
        {
            var receiverIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (receiverIdClaim == null) return;

            int receiverId = int.Parse(receiverIdClaim);

            var messages = await _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            messages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
        }
    }
}
