using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineJobRecruitmentSystem.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int receiverId, string content)
        {
            var senderIdClaim = Context.User?.FindFirst("userId")?.Value;
            if (senderIdClaim == null) return;

            int senderId = int.Parse(senderIdClaim);

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.SenderId,
                message.Content,
                message.CreatedAt
            });

            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.SenderId,
                message.Content,
                message.CreatedAt
            });
        }

        public async Task MarkAsRead(int senderId)
        {
            var receiverIdClaim = Context.User?.FindFirst("userId")?.Value;
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
