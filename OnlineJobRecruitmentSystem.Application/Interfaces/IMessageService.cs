using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IMessageService
    {
        Task<Message> SaveMessageAsync(int senderId, SendMessageDto dto);
        Task<List<ReturnMessageDto>> GetConversationAsync(int userId, int otherUserId);
        Task<List<ReturnMessageDto>> GetInboxAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}