using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobApplicationController(
        IJobApplicationService jobApplicationService,
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

            var applyResult = await jobApplicationService.ApplyAsync(CurrentUserId, jobId, dto);

            return applyResult switch
            {
                JobApplicationResult.ProfileNotFound => NotFound(ResponseModel<string>.Fail("Job seeker profile not found.")),
                JobApplicationResult.JobNotFound => NotFound(ResponseModel<string>.Fail("Job not found.")),
                JobApplicationResult.AlreadyApplied => BadRequest(ResponseModel<string>.Fail("You already applied to this job.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Application submitted."))
            };
        }

        [HttpGet("mine")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
        public async Task<IActionResult> GetMyApplications()
        {
            var list = await jobApplicationService.GetMyApplicationsAsync(CurrentUserId);
            if (list == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            return Ok(ResponseModel<List<ReturnJobApplicationDto>>.Ok(list));
        }
    }
}