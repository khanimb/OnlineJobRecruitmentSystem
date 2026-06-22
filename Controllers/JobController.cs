using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Data;
using OnlineJobRecruitmentSystem.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Models;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController(
        AppDbContext context,
        IValidator<CreateJobDto> createValidator,
        IValidator<UpdateJobDto> updateValidator
    ) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? category,
            [FromQuery] string? location,
            [FromQuery] string? jobType,
            [FromQuery] decimal? salaryMin,
            [FromQuery] decimal? salaryMax)
        {
            var query = context.JobPosts
                .Include(j => j.EmployerProfile)
                .Where(j => j.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(j => j.Category == category);
            if (!string.IsNullOrEmpty(location))
                query = query.Where(j => j.Location == location);
            if (!string.IsNullOrEmpty(jobType))
                query = query.Where(j => j.JobType == jobType);
            if (salaryMin.HasValue)
                query = query.Where(j => j.SalaryMin >= salaryMin);
            if (salaryMax.HasValue)
                query = query.Where(j => j.SalaryMax <= salaryMax);

            var jobs = await query.Select(j => new ReturnJobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Location = j.Location,
                JobType = j.JobType,
                Category = j.Category,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                Deadline = j.Deadline,
                IsActive = j.IsActive,
                CompanyName = j.EmployerProfile!.CompanyName
            }).ToListAsync();

            return Ok(ResponseModel<List<ReturnJobDto>>.Ok(jobs));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await context.JobPosts
                .Include(j => j.EmployerProfile)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            var dto = new ReturnJobDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                Location = job.Location,
                JobType = job.JobType,
                Category = job.Category,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                Deadline = job.Deadline,
                IsActive = job.IsActive,
                CompanyName = job.EmployerProfile!.CompanyName
            };

            return Ok(ResponseModel<ReturnJobDto>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> CreateJob(CreateJobDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
                return BadRequest(ResponseModel<string>.Fail("Employer profile not found."));

            var job = new JobPost
            {
                EmployerProfileId = employer.Id,
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Location = dto.Location,
                JobType = dto.JobType,
                Category = dto.Category,
                SalaryMin = dto.SalaryMin,
                SalaryMax = dto.SalaryMax,
                Deadline = dto.Deadline
            };

            context.JobPosts.Add(job);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job posted successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateJob(int id, UpdateJobDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            var job = await context.JobPosts
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == employer!.Id);

            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            job.Title = dto.Title;
            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.Location = dto.Location;
            job.JobType = dto.JobType;
            job.Category = dto.Category;
            job.SalaryMin = dto.SalaryMin;
            job.SalaryMax = dto.SalaryMax;
            job.Deadline = dto.Deadline;
            job.IsActive = dto.IsActive;

            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job updated successfully."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            var job = await context.JobPosts
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == employer!.Id);

            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            context.JobPosts.Remove(job);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job deleted successfully."));
        }
    }
}