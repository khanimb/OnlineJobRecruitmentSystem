using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public MessageService(AppDbContext context, IMapper mapper, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
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

            await _notificationService.CreateNotificationAsync(new OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos.CreateNotificationDto
            {
                UserId = dto.ReceiverId,
                Title = "New Message",
                Message = "You have received a new message.",
                Type = "Message"
            });

            return message;
        }

        public async Task<List<ReturnMessageDto>> GetConversationAsync(int userId, int otherUserId)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.CreatedAt)
                .ProjectTo<ReturnMessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<ReturnMessageDto>> GetInboxAsync(int userId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ProjectTo<ReturnMessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }
    }
}