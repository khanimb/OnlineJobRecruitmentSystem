using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public JobAlertService(AppDbContext context, IEmailService emailService, IMapper mapper)
        {
            _context = context;
            _emailService = emailService;
            _mapper = mapper;
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

            return _mapper.Map<ReturnJobAlertDto>(alert);
        }

        public async Task<List<ReturnJobAlertDto>> GetUserAlertsAsync(int userId)
        {
            return await _context.JobAlerts
                .Where(a => a.UserId == userId)
                .ProjectTo<ReturnJobAlertDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ReturnJobAlertDto?> GetAlertByIdAsync(int alertId, int userId)
        {
            var alert = await _context.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);

            return alert == null ? null : _mapper.Map<ReturnJobAlertDto>(alert);
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

            if (!alerts.Any()) return;

            var activeJobs = await _context.JobPosts
                .Where(j => j.IsActive)
                .ToListAsync();

            foreach (var alert in alerts)
            {
                var jobs = activeJobs
                    .Where(j =>
                        (string.IsNullOrEmpty(alert.Keyword) || j.Title.Contains(alert.Keyword)) &&
                        (string.IsNullOrEmpty(alert.Location) || j.Location.Contains(alert.Location)))
                    .Take(5)
                    .ToList();

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
    }
}