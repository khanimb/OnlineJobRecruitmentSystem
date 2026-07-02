using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.AnalyticsDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employer")]
    public class AnalyticsController(
        AppDbContext context,
        IAnalyticsService analyticsService) : ControllerBase
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

            var stats = await analyticsService.GetEmployerStatsAsync(employer.Id);
            return Ok(ResponseModel<EmployerStatsDto>.Ok(stats));
        }
    }
}