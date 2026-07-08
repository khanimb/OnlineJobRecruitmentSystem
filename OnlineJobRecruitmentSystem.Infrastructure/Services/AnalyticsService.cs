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
            var jobIdsQuery = _context.JobPosts
                .Where(j => j.EmployerProfileId == employerId)
                .Select(j => j.Id);

            var applicationsQuery = _context.JobApplications
                .Where(a => jobIdsQuery.Contains(a.JobPostId));

            var totalJobs = await jobIdsQuery.CountAsync();
            var totalApplications = await applicationsQuery.CountAsync();
            var pending = await applicationsQuery.CountAsync(a => a.Status == Domain.Enums.ApplicationStatus.Applied);
            var shortlisted = await applicationsQuery.CountAsync(a => a.Status == Domain.Enums.ApplicationStatus.Shortlisted);
            var rejected = await applicationsQuery.CountAsync(a => a.Status == Domain.Enums.ApplicationStatus.Rejected);

            var trendRaw = await applicationsQuery
                .GroupBy(a => a.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var trend = trendRaw
                .Select(t => new ApplicationTrendDto { Date = t.Date.ToString("yyyy-MM-dd"), Count = t.Count })
                .ToList();

            return new EmployerStatsDto
            {
                TotalJobs = totalJobs,
                TotalApplications = totalApplications,
                Pending = pending,
                Shortlisted = shortlisted,
                Rejected = rejected,
                ApplicationTrend = trend
            };
        }
    }
}