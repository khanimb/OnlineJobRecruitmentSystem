using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.AnalyticsDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployerStatsDto> GetEmployerStatsAsync(int employerId)
        {
            var jobs = await _context.JobPosts
                .Where(j => j.EmployerProfileId == employerId)
                .ToListAsync();

            var jobIds = jobs.Select(j => j.Id).ToList();

            var applications = await _context.JobApplications
                .Where(a => jobIds.Contains(a.JobPostId))
                .ToListAsync();

            var trend = applications
                .GroupBy(a => a.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ApplicationTrendDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .ToList();

            return new EmployerStatsDto
            {
                TotalJobs = jobs.Count,
                TotalApplications = applications.Count,
                Pending = applications.Count(a => a.Status == Domain.Enums.ApplicationStatus.Applied),
                Shortlisted = applications.Count(a => a.Status == Domain.Enums.ApplicationStatus.Shortlisted),
                Rejected = applications.Count(a => a.Status == Domain.Enums.ApplicationStatus.Rejected),
                ApplicationTrend = trend
            };
        }
    }
}