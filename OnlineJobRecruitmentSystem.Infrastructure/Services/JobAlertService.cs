using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class JobAlertService : IJobAlertService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public JobAlertService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<ReturnJobAlertDto> CreateAlertAsync(int userId, CreateJobAlertDto dto)
        {
            var alert = new JobAlert
            {
                UserId = userId,
                Keyword = dto.Keyword ?? string.Empty,
                Location = dto.Location ?? string.Empty,
                Frequency = dto.Frequency ?? string.Empty,
                IsActive = true
            };

            _context.JobAlerts.Add(alert);
            await _context.SaveChangesAsync();

            return MapToDto(alert);
        }

        public async Task<List<ReturnJobAlertDto>> GetUserAlertsAsync(int userId)
        {
            return await _context.JobAlerts
                .Where(a => a.UserId == userId)
                .Select(a => new ReturnJobAlertDto
                {
                    Id = a.Id,
                    Keyword = a.Keyword,
                    Location = a.Location,
                    Frequency = a.Frequency,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ReturnJobAlertDto?> GetAlertByIdAsync(int alertId, int userId)
        {
            var alert = await _context.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);

            return alert == null ? null : MapToDto(alert);
        }

        public async Task<bool> UpdateAlertAsync(int alertId, int userId, UpdateJobAlertDto dto)
        {
            var alert = await _context.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);

            if (alert == null) return false;

            alert.Keyword = dto.Keyword ?? string.Empty;
            alert.Location = dto.Location ?? string.Empty;
            alert.Frequency = dto.Frequency ?? string.Empty;
            alert.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAlertAsync(int alertId, int userId)
        {
            var alert = await _context.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);

            if (alert == null) return false;

            _context.JobAlerts.Remove(alert);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SendJobAlertsAsync(string frequency)
        {
            var alerts = await _context.JobAlerts
                .Include(a => a.User)
                .Where(a => a.IsActive && a.Frequency == frequency)
                .ToListAsync();

            foreach (var alert in alerts)
            {
                var jobs = await _context.JobPosts
                    .Where(j => j.IsActive &&
                        (string.IsNullOrEmpty(alert.Keyword) || j.Title.Contains(alert.Keyword)) &&
                        (string.IsNullOrEmpty(alert.Location) || j.Location.Contains(alert.Location)))
                    .Take(5)
                    .ToListAsync();

                if (jobs.Any())
                {
                    var jobList = string.Join("<br/>", jobs.Select(j =>
                        $"<li><b>{j.Title}</b> — {j.Location}</li>"));

                    await _emailService.SendEmailAsync(
                        alert.User.Email,
                        $"Your {frequency} job alerts",
                        $"<h3>New jobs matching your alert:</h3><ul>{jobList}</ul>"
                    );
                }
            }
        }

        private static ReturnJobAlertDto MapToDto(JobAlert alert) => new ReturnJobAlertDto
        {
            Id = alert.Id,
            Keyword = alert.Keyword,
            Location = alert.Location,
            Frequency = alert.Frequency,
            IsActive = alert.IsActive,
            CreatedAt = alert.CreatedAt
        };
    }
}