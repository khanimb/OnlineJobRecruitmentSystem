using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Data;
using OnlineJobRecruitmentSystem.DTOs.ApplicationDtos;
using OnlineJobRecruitmentSystem.Models;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationController(
        AppDbContext context,
        IValidator<CreateApplicationDto> createValidator,
        IValidator<UpdateApplicationStatusDto> updateValidator
    ) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost("{jobId}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int jobId, CreateApplicationDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var job = await context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);
            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            if (await context.Applications.AnyAsync(a => a.JobPostId == jobId && a.JobSeekerProfileId == profile.Id))
                return BadRequest(ResponseModel<string>.Fail("You already applied to this job."));

            context.Applications.Add(new Application
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
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = GetUserId();

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var list = await context.Applications
                .Include(a => a.JobPost)
                    .ThenInclude(j => j!.EmployerProfile)
                .Where(a => a.JobSeekerProfileId == profile.Id)
                .Select(a => new ReturnApplicationDto
                {
                    Id = a.Id,
                    JobPostId = a.JobPostId,
                    JobTitle = a.JobPost!.Title,
                    CompanyName = a.JobPost.EmployerProfile!.CompanyName,
                    Status = a.Status.ToString(),
                    CoverLetter = a.CoverLetter,
                    AppliedAt = a.AppliedAt
                }).ToListAsync();

            return Ok(ResponseModel<List<ReturnApplicationDto>>.Ok(list));
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateApplicationStatusDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();

            var employer = await context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var application = await context.Applications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == id && a.JobPost!.EmployerProfileId == employer.Id);

            if (application == null)
                return NotFound(ResponseModel<string>.Fail("Application not found."));

            application.Status = dto.Status;
            application.Notes = dto.Notes;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Status updated."));
        }
    }
}
