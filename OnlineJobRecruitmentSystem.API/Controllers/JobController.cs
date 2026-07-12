using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController(
        IJobService jobService,
        IValidator<CreateJobDto> createValidator,
        IValidator<UpdateJobDto> updateValidator
    ) : BaseApiController
    {
        [HttpGet("stats")]
        public async Task<IActionResult> GetPublicStats()
        {
            var stats = await jobService.GetPublicStatsAsync();
            return Ok(ResponseModel<PlatformStatsDto>.Ok(stats));
        }

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
            var result = await jobService.GetJobsAsync(category, location, jobType, salaryMin, salaryMax, page, pageSize);
            return Ok(ResponseModel<object>.Ok(result));
        }

        [HttpGet("my-jobs")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> GetMyJobs()
        {
            var jobs = await jobService.GetMyJobsAsync(CurrentUserId);
            return Ok(ResponseModel<List<ReturnJobDto>>.Ok(jobs));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            return Ok(ResponseModel<ReturnJobDto>.Ok(job));
        }

        [HttpPost]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> CreateJob(CreateJobDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var opResult = await jobService.CreateJobAsync(CurrentUserId, dto);
            if (opResult == JobOperationResult.EmployerNotFound)
                return BadRequest(ResponseModel<string>.Fail("Employer profile not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Job posted successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> UpdateJob(int id, UpdateJobDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var opResult = await jobService.UpdateJobAsync(id, CurrentUserId, dto);

            return opResult switch
            {
                JobOperationResult.EmployerNotFound => NotFound(ResponseModel<string>.Fail("Employer profile not found.")),
                JobOperationResult.JobNotFound => NotFound(ResponseModel<string>.Fail("Job not found.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Job updated successfully."))
            };
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var opResult = await jobService.DeleteJobAsync(id, CurrentUserId);

            return opResult switch
            {
                JobOperationResult.EmployerNotFound => NotFound(ResponseModel<string>.Fail("Employer profile not found.")),
                JobOperationResult.JobNotFound => NotFound(ResponseModel<string>.Fail("Job not found.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Job deleted successfully."))
            };
        }

        [HttpGet("{id}/similar")]
        public async Task<IActionResult> GetSimilarJobs(int id)
        {
            var jobs = await jobService.GetSimilarJobsAsync(id);
            if (jobs == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            return Ok(ResponseModel<List<ReturnJobDto>>.Ok(jobs));
        }
    }
}