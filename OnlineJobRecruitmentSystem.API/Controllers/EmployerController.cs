using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.EmployerDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
    public class EmployerController(
        IEmployerService employerService,
        IValidator<CreateEmployerDto> createValidator,
        IValidator<UpdateEmployerDto> updateValidator,
        IValidator<UpdateJobApplicationStatusDto> statusValidator,
        IEmailService emailService,
        INotificationService notificationService
    ) : BaseApiController
    {
        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile(CreateEmployerDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var profile = await employerService.CreateProfileAsync(CurrentUserId, dto);
            if (profile == null)
                return BadRequest(ResponseModel<string>.Fail("Employer profile already exists."));

            return Ok(ResponseModel<ReturnEmployerDto>.Ok(profile, "Employer profile created."));
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await employerService.GetProfileAsync(CurrentUserId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            return Ok(ResponseModel<ReturnEmployerDto>.Ok(profile));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateEmployerDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var profile = await employerService.UpdateProfileAsync(CurrentUserId, dto);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            return Ok(ResponseModel<ReturnEmployerDto>.Ok(profile, "Employer profile updated."));
        }

        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ResponseModel<string>.Fail("File is required."));

            if (!file.IsValidType(".jpg", ".jpeg", ".png"))
                return BadRequest(ResponseModel<string>.Fail("Only JPG, JPEG, PNG files are allowed."));

            if (!file.IsValidSize(2 * 1024 * 1024))
                return BadRequest(ResponseModel<string>.Fail("File size must not exceed 2 MB."));

            if (!file.HasValidSignature(".jpg", ".jpeg", ".png"))
                return BadRequest(ResponseModel<string>.Fail("File content does not match its extension."));

            var logoUrl = await employerService.UploadLogoAsync(CurrentUserId, file);
            if (logoUrl == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            return Ok(ResponseModel<string>.Ok(logoUrl, "Logo uploaded successfully."));
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications([FromQuery] string? status)
        {
            var list = await employerService.GetApplicationsAsync(CurrentUserId, status);
            if (list == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            return Ok(ResponseModel<List<ReturnEmployerApplicationDto>>.Ok(list));
        }

        [HttpGet("applications/{jobId}")]
        public async Task<IActionResult> GetApplicationsByJob(int jobId)
        {
            var result = await employerService.GetApplicationsByJobAsync(CurrentUserId, jobId);

            if (result.EmployerNotFound)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));
            if (result.JobNotFound)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            return Ok(ResponseModel<List<ReturnJobApplicantDto>>.Ok(result.Applicants));
        }

        [HttpPut("applications/{id}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, UpdateJobApplicationStatusDto dto)
        {
            var result = await statusValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var updateResult = await employerService.UpdateApplicationStatusAsync(CurrentUserId, id, dto.Status, dto.Notes);

            if (updateResult.EmployerNotFound)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));
            if (updateResult.ApplicationNotFound)
                return NotFound(ResponseModel<string>.Fail("Application not found."));
            if (updateResult.InvalidTransition)
                return BadRequest(ResponseModel<string>.Fail($"Cannot change status from {updateResult.CurrentStatus} to {dto.Status}."));

            if (!string.IsNullOrEmpty(updateResult.JobSeekerEmail))
            {
                await emailService.SendEmailAsync(
                    updateResult.JobSeekerEmail,
                    "Application Status Updated",
                    $"<h3>Your application status has been updated.</h3>" +
                    $"<p>Job: <b>{updateResult.JobTitle}</b></p>" +
                    $"<p>New Status: <b>{updateResult.NewStatus}</b></p>"
                );

                await notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = updateResult.JobSeekerUserId,
                    Title = "Application Status Updated",
                    Message = $"Your application for '{updateResult.JobTitle}' is now: {updateResult.NewStatus}.",
                    Type = "ApplicationStatus"
                });
            }

            return Ok(ResponseModel<string>.Ok(null!, "Application status updated."));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await employerService.GetDashboardAsync(CurrentUserId);
            if (dashboard == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            return Ok(ResponseModel<EmployerDashboardDto>.Ok(dashboard));
        }
    }
}