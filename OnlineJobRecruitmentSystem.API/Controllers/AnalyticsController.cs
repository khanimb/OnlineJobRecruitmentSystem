using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.AnalyticsDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employer")]
    public class AnalyticsController(AppDbContext context) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var userId = GetUserId();

            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var jobs = await context.JobPosts
                .Where(j => j.EmployerProfileId == employer.Id)
                .ToListAsync();

            var jobIds = jobs.Select(j => j.Id).ToList();

            var applications = await context.JobApplications
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

            return Ok(ResponseModel<EmployerStatsDto>.Ok(new EmployerStatsDto
            {
                TotalJobs = jobs.Count,
                TotalApplications = applications.Count,
                Pending = applications.Count(a => a.Status == ApplicationStatus.Applied),
                Shortlisted = applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
                Rejected = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                ApplicationTrend = trend
            }));
        }
    }
}