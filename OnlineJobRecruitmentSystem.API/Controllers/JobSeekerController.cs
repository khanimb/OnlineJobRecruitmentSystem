using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
    public class JobSeekerController(
        AppDbContext context,
        IValidator<CreateJobSeekerDto> createValidator,
        IValidator<UpdateJobSeekerDto> updateValidator,
        FileManager fileManager 
    ) : BaseApiController
    {
        

        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile(CreateJobSeekerDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = CurrentUserId;

            if (await context.JobSeekerProfiles.AnyAsync(j => j.UserId == userId))
                return BadRequest(ResponseModel<string>.Fail("Job seeker profile already exists."));

            var profile = new JobSeekerProfile
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Skills = dto.Skills,
                WorkExperience = dto.WorkExperience
            };

            context.JobSeekerProfiles.Add(profile);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(new ReturnJobSeekerDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Skills = profile.Skills,
                WorkExperience = profile.WorkExperience,
                CvUrl = profile.CvUrl
            }, "Job seeker profile created."));
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(new ReturnJobSeekerDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Skills = profile.Skills,
                WorkExperience = profile.WorkExperience,
                CvUrl = profile.CvUrl
            }));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateJobSeekerDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            profile.FullName = dto.FullName;
            profile.Phone = dto.Phone;
            profile.Skills = dto.Skills;
            profile.WorkExperience = dto.WorkExperience;

            await context.SaveChangesAsync();

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(new ReturnJobSeekerDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Skills = profile.Skills,
                WorkExperience = profile.WorkExperience,
                CvUrl = profile.CvUrl
            }, "Job seeker profile updated."));
        }

        [HttpPost("upload-cv")]
        public async Task<IActionResult> UploadCv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ResponseModel<string>.Fail("File is required."));

            if (!file.IsValidType(".pdf", ".doc", ".docx"))
                return BadRequest(ResponseModel<string>.Fail("Only PDF, DOC, DOCX files are allowed."));

            if (!file.IsValidSize(5 * 1024 * 1024))
                return BadRequest(ResponseModel<string>.Fail("File size must not exceed 5 MB."));

            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            if (!string.IsNullOrEmpty(profile.CvUrl))
                fileManager.Delete(profile.CvUrl);

            profile.CvUrl = await fileManager.UploadAsync(file, "cvs");
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(profile.CvUrl, "CV uploaded successfully."));
        }

        [HttpGet("profile/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfileById(int userId)
        {
            var profile = await context.JobSeekerProfiles
                .FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Profile not found."));

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(new ReturnJobSeekerDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Skills = profile.Skills,
                WorkExperience = profile.WorkExperience,
                CvUrl = profile.CvUrl
            }));
        }

        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedJobs()
        {
            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var saved = await context.SavedJobs
                .Include(s => s.JobPost)
                    .ThenInclude(j => j!.EmployerProfile)
                .Where(s => s.JobSeekerProfileId == profile.Id)
                .Select(s => new
                {
                    s.Id,
                    s.SavedAt,
                    Job = new
                    {
                        s.JobPost!.Id,
                        s.JobPost.Title,
                        s.JobPost.Location,
                        s.JobPost.JobType,
                        s.JobPost.SalaryMin,
                        s.JobPost.SalaryMax,
                        s.JobPost.Deadline,
                        CompanyName = s.JobPost.EmployerProfile!.CompanyName
                    }
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(saved));
        }

        [HttpPost("saved/{jobId}")]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var job = await context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);
            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            if (await context.SavedJobs.AnyAsync(s => s.JobSeekerProfileId == profile.Id && s.JobPostId == jobId))
                return BadRequest(ResponseModel<string>.Fail("Job already saved."));

            context.SavedJobs.Add(new SavedJob
            {
                JobSeekerProfileId = profile.Id,
                JobPostId = jobId
            });

            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job saved."));
        }

        [HttpDelete("saved/{jobId}")]
        public async Task<IActionResult> RemoveSavedJob(int jobId)
        {
            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var saved = await context.SavedJobs
                .FirstOrDefaultAsync(s => s.JobSeekerProfileId == profile.Id && s.JobPostId == jobId);

            if (saved == null)
                return NotFound(ResponseModel<string>.Fail("Saved job not found."));

            context.SavedJobs.Remove(saved);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job removed from saved list."));
        }
    }
}
