using OnlineJobRecruitmentSystem.Application.DTOs.ContactDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IContactService
    {
        Task SendContactMessageAsync(ContactMessageDto dto);
    }
}