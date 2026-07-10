using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController(
        AppDbContext context,
        IValidator<CreateJobDto> createValidator,
        IValidator<UpdateJobDto> updateValidator
    ) : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? category,
            [FromQuery] string? location,
            [FromQuery] string? jobType,
            [FromQuery] decimal? salaryMin,
            [FromQuery] decimal? salaryMax,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = context.JobPosts
                .Include(j => j.EmployerProfile)
                .Include(j => j.Applications)
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

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new ReturnJobDto
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
                    CompanyName = j.EmployerProfile!.CompanyName,
                    ApplicationCount = j.Applications.Count
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Data = jobs
            }));
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
                CompanyName = job.EmployerProfile!.CompanyName,
                ApplicationCount = await context.JobApplications.CountAsync(a => a.JobPostId == job.Id)
            };

            return Ok(ResponseModel<ReturnJobDto>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> CreateJob(CreateJobDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = CurrentUserId;
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
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> UpdateJob(int id, UpdateJobDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = CurrentUserId;
            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var job = await context.JobPosts
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == employer.Id);

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
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = CurrentUserId;
            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var job = await context.JobPosts
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == employer.Id);

            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            context.JobPosts.Remove(job);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job deleted successfully."));
        }

        [HttpGet("{id}/similar")]
        public async Task<IActionResult> GetSimilarJobs(int id)
        {
            var job = await context.JobPosts.FindAsync(id);
            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            var similarJobs = await context.JobPosts
                .Include(j => j.EmployerProfile)
                .Include(j => j.Applications)
                .Where(j => j.IsActive && j.Id != id &&
                            (j.Category == job.Category || j.JobType == job.JobType))
                .Take(5)
                .Select(j => new ReturnJobDto
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
                    CompanyName = j.EmployerProfile!.CompanyName,
                    ApplicationCount = j.Applications.Count
                }).ToListAsync();

            return Ok(ResponseModel<List<ReturnJobDto>>.Ok(similarJobs));
        }
    }
}