using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Data;
using OnlineJobRecruitmentSystem.DTOs.ApplicationDtos;
using OnlineJobRecruitmentSystem.DTOs.EmployerDtos;
using OnlineJobRecruitmentSystem.Models;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employer")]
    public class EmployerController(
        AppDbContext context,
        IValidator<CreateEmployerDto> createValidator,
        IValidator<UpdateEmployerDto> updateValidator,
        IValidator<UpdateApplicationStatusDto> statusValidator
    ) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile(CreateEmployerDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();

            if (await context.EmployerProfiles.AnyAsync(e => e.UserId == userId))
                return BadRequest(ResponseModel<string>.Fail("Employer profile already exists."));

            var profile = new EmployerProfile
            {
                UserId = userId,
                CompanyName = dto.CompanyName,
                Description = dto.Description,
                Website = dto.Website
            };

            context.EmployerProfiles.Add(profile);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<ReturnEmployerDto>.Ok(new ReturnEmployerDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                CompanyName = profile.CompanyName,
                Description = profile.Description,
                Website = profile.Website,
                LogoUrl = profile.LogoUrl
            }, "Employer profile created."));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateEmployerDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();

            var profile = await context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            profile.CompanyName = dto.CompanyName;
            profile.Description = dto.Description;
            profile.Website = dto.Website;

            await context.SaveChangesAsync();

            return Ok(ResponseModel<ReturnEmployerDto>.Ok(new ReturnEmployerDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                CompanyName = profile.CompanyName,
                Description = profile.Description,
                Website = profile.Website,
                LogoUrl = profile.LogoUrl
            }, "Employer profile updated."));
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications([FromQuery] string? status)
        {
            var userId = GetUserId();

            var employer = await context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var query = context.Applications
                .Include(a => a.JobPost)
                .Include(a => a.JobSeekerProfile)
                .Where(a => a.JobPost!.EmployerProfileId == employer.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            var list = await query.Select(a => new
            {
                a.Id,
                a.Status,
                a.CoverLetter,
                a.Notes,
                a.AppliedAt,
                JobTitle = a.JobPost!.Title,
                JobSeeker = new
                {
                    a.JobSeekerProfile!.FullName,
                    a.JobSeekerProfile.Skills,
                    a.JobSeekerProfile.CvUrl
                }
            }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(list));
        }

        [HttpGet("applications/{jobId}")]
        public async Task<IActionResult> GetApplicationsByJob(int jobId)
        {
            var userId = GetUserId();

            var employer = await context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var job = await context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == employer.Id);
            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            var list = await context.Applications
                .Include(a => a.JobSeekerProfile)
                .Where(a => a.JobPostId == jobId)
                .Select(a => new
                {
                    a.Id,
                    a.Status,
                    a.CoverLetter,
                    a.Notes,
                    a.AppliedAt,
                    JobSeeker = new
                    {
                        a.JobSeekerProfile!.FullName,
                        a.JobSeekerProfile.Skills,
                        a.JobSeekerProfile.CvUrl
                    }
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(list));
        }

        [HttpPut("applications/{id}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, UpdateApplicationStatusDto dto)
        {
            var result = await statusValidator.ValidateAsync(dto);
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

            return Ok(ResponseModel<string>.Ok(null!, "Application status updated."));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetUserId();

            var employer = await context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var totalJobs = await context.JobPosts.CountAsync(j => j.EmployerProfileId == employer.Id);
            var totalApplications = await context.Applications
                .Include(a => a.JobPost)
                .CountAsync(a => a.JobPost!.EmployerProfileId == employer.Id);
            var shortlisted = await context.Applications
                .Include(a => a.JobPost)
                .CountAsync(a => a.JobPost!.EmployerProfileId == employer.Id && a.Status == "Shortlisted");
            var rejected = await context.Applications
                .Include(a => a.JobPost)
                .CountAsync(a => a.JobPost!.EmployerProfileId == employer.Id && a.Status == "Rejected");

            return Ok(ResponseModel<object>.Ok(new
            {
                TotalJobs = totalJobs,
                TotalApplications = totalApplications,
                Shortlisted = shortlisted,
                Rejected = rejected
            }));
        }
    }
}
