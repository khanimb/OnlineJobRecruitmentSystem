using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
    public class JobSeekerController(
        IJobSeekerService jobSeekerService,
        IValidator<CreateJobSeekerDto> createValidator,
        IValidator<UpdateJobSeekerDto> updateValidator
    ) : BaseApiController
    {
        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile(CreateJobSeekerDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var profile = await jobSeekerService.CreateProfileAsync(CurrentUserId, dto);
            if (profile == null)
                return BadRequest(ResponseModel<string>.Fail("Job seeker profile already exists."));

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(profile, "Job seeker profile created."));
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await jobSeekerService.GetProfileAsync(CurrentUserId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(profile));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateJobSeekerDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var profile = await jobSeekerService.UpdateProfileAsync(CurrentUserId, dto);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(profile, "Job seeker profile updated."));
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

            if (!file.HasValidSignature(".pdf", ".doc", ".docx"))
                return BadRequest(ResponseModel<string>.Fail("File content does not match its extension."));

            var cvUrl = await jobSeekerService.UploadCvAsync(CurrentUserId, file);
            if (cvUrl == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            return Ok(ResponseModel<string>.Ok(cvUrl, "CV uploaded successfully."));
        }

        [HttpGet("profile/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfileById(int userId)
        {
            var profile = await jobSeekerService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Profile not found."));

            return Ok(ResponseModel<ReturnJobSeekerDto>.Ok(profile));
        }

        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedJobs()
        {
            var saved = await jobSeekerService.GetSavedJobsAsync(CurrentUserId);
            if (saved == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            return Ok(ResponseModel<List<ReturnSavedJobDto>>.Ok(saved));
        }

        [HttpPost("saved/{jobId}")]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            var result = await jobSeekerService.SaveJobAsync(CurrentUserId, jobId);

            return result switch
            {
                SaveJobResult.ProfileNotFound => NotFound(ResponseModel<string>.Fail("Job seeker profile not found.")),
                SaveJobResult.JobNotFound => NotFound(ResponseModel<string>.Fail("Job not found.")),
                SaveJobResult.AlreadySaved => BadRequest(ResponseModel<string>.Fail("Job already saved.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Job saved."))
            };
        }

        [HttpDelete("saved/{jobId}")]
        public async Task<IActionResult> RemoveSavedJob(int jobId)
        {
            var result = await jobSeekerService.RemoveSavedJobAsync(CurrentUserId, jobId);

            return result switch
            {
                RemoveSavedJobResult.ProfileNotFound => NotFound(ResponseModel<string>.Fail("Job seeker profile not found.")),
                RemoveSavedJobResult.SavedJobNotFound => NotFound(ResponseModel<string>.Fail("Saved job not found.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Job removed from saved list."))
            };
        }
    }
}