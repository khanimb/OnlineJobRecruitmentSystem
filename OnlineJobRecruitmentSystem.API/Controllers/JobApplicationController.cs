using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.ApplicationDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobApplicationController(
        AppDbContext context,
        IValidator<CreateJobApplicationDto> createValidator
    ) : BaseApiController
    {
        

        [HttpPost("{jobId}")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
        public async Task<IActionResult> Apply(int jobId, CreateJobApplicationDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var job = await context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);
            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            if (await context.JobApplications.AnyAsync(a => a.JobPostId == jobId && a.JobSeekerProfileId == profile.Id))
                return BadRequest(ResponseModel<string>.Fail("You already applied to this job."));

            context.JobApplications.Add(new JobApplication
            {
                JobPostId = jobId,
                JobSeekerProfileId = profile.Id,
                CoverLetter = dto.CoverLetter,
                Status = ApplicationStatus.Applied
            });

            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Application submitted."));
        }

        [HttpGet("mine")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var list = await context.JobApplications
                .Include(a => a.JobPost)
                    .ThenInclude(j => j!.EmployerProfile)
                .Where(a => a.JobSeekerProfileId == profile.Id)
                .Select(a => new ReturnJobApplicationDto
                {
                    Id = a.Id,
                    JobPostId = a.JobPostId,
                    JobTitle = a.JobPost!.Title,
                    CompanyName = a.JobPost.EmployerProfile!.CompanyName,
                    Status = a.Status.ToString(),
                    CoverLetter = a.CoverLetter,
                    AppliedAt = a.AppliedAt
                }).ToListAsync();

            return Ok(ResponseModel<List<ReturnJobApplicationDto>>.Ok(list));
        }
    }
}
