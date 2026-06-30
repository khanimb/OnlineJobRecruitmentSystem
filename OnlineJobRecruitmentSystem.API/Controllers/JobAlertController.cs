using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobAlertController(
        AppDbContext context,
        IValidator<CreateJobAlertDto> createValidator,
        IValidator<UpdateJobAlertDto> updateValidator) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> CreateAlert(CreateJobAlertDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();

            var alert = new JobAlert
            {
                UserId = userId,
                Keyword = dto.Keyword ?? string.Empty,
                Location = dto.Location ?? string.Empty,
                Frequency = dto.Frequency ?? string.Empty,
                IsActive = true
            };

            context.JobAlerts.Add(alert);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job alert created."));
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            var userId = GetUserId();

            var alerts = await context.JobAlerts
                .Where(a => a.UserId == userId)
                .Select(a => new ReturnJobAlertDto
                {
                    Id = a.Id,
                    Keyword = a.Keyword,
                    Location = a.Location,
                    Frequency = a.Frequency,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnJobAlertDto>>.Ok(alerts));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlert(int id)
        {
            var userId = GetUserId();

            var alert = await context.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (alert == null)
                return NotFound(ResponseModel<string>.Fail("Alert not found."));

            return Ok(ResponseModel<ReturnJobAlertDto>.Ok(new ReturnJobAlertDto
            {
                Id = alert.Id,
                Keyword = alert.Keyword,
                Location = alert.Location,
                Frequency = alert.Frequency,
                IsActive = alert.IsActive,
                CreatedAt = alert.CreatedAt
            }));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlert(int id, UpdateJobAlertDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();

            var alert = await context.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (alert == null)
                return NotFound(ResponseModel<string>.Fail("Alert not found."));

            alert.Keyword = dto.Keyword ?? string.Empty;
            alert.Location = dto.Location ?? string.Empty;
            alert.Frequency = dto.Frequency ?? string.Empty;
            alert.IsActive = dto.IsActive;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Alert updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            var userId = GetUserId();

            var alert = await context.JobAlerts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (alert == null)
                return NotFound(ResponseModel<string>.Fail("Alert not found."));

            context.JobAlerts.Remove(alert);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Alert deleted."));
        }
    }
}