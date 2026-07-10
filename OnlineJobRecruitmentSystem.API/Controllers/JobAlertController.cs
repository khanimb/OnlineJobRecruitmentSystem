using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobAlertController(
        IJobAlertService jobAlertService,
        IValidator<CreateJobAlertDto> createValidator,
        IValidator<UpdateJobAlertDto> updateValidator) : BaseApiController
    {
        

        [HttpPost]
        public async Task<IActionResult> CreateAlert(CreateJobAlertDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            await jobAlertService.CreateAlertAsync(CurrentUserId, dto);
            return Ok(ResponseModel<string>.Ok(null!, "Job alert created."));
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            var alerts = await jobAlertService.GetUserAlertsAsync(CurrentUserId);
            return Ok(ResponseModel<List<ReturnJobAlertDto>>.Ok(alerts));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlert(int id)
        {
            var alert = await jobAlertService.GetAlertByIdAsync(id, CurrentUserId);
            if (alert == null)
                return NotFound(ResponseModel<string>.Fail("Alert not found."));

            return Ok(ResponseModel<ReturnJobAlertDto>.Ok(alert));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlert(int id, UpdateJobAlertDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var updated = await jobAlertService.UpdateAlertAsync(id, CurrentUserId, dto);
            if (!updated)
                return NotFound(ResponseModel<string>.Fail("Alert not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Alert updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            var deleted = await jobAlertService.DeleteAlertAsync(id, CurrentUserId);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("Alert not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Alert deleted."));
        }
    }
}