using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;

        public MessageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Message> SaveMessageAsync(int senderId, SendMessageDto dto)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content ?? string.Empty,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<ReturnMessageDto>> GetConversationAsync(int userId, int otherUserId)
        {
            return await _context.Messages
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
        }

        public async Task<List<ReturnMessageDto>> GetInboxAsync(int userId)
        {
            return await _context.Messages
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
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }
    }
}