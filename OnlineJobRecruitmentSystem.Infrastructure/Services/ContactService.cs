using Microsoft.Extensions.Configuration;
using OnlineJobRecruitmentSystem.Application.DTOs.ContactDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class ContactService(IEmailService emailService, IConfiguration configuration, AppDbContext context) : IContactService
    {
        public async Task SendContactMessageAsync(ContactMessageDto dto)
        {
            context.ContactMessages.Add(new ContactMessage { Name = dto.Name, Email = dto.Email, Message = dto.Message });
            await context.SaveChangesAsync();

            var adminEmail = configuration["EmailSettings:From"]!;
            var body = $"<p><b>From:</b> {dto.Name} ({dto.Email})</p><p>{dto.Message}</p>";
            await emailService.SendEmailAsync(adminEmail, "New contact message — NexHire", body);
        }
    }
}