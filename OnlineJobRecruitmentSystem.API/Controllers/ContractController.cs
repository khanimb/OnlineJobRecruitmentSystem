using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.API.Hubs;
using OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractController(
        AppDbContext context,
        IHubContext<NotificationHub> notificationHub,
        IValidator<CreateContractDto> createValidator,
        IValidator<UpdateContractDto> updateValidator) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> CreateContract(CreateContractDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();

            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var job = await context.JobPosts
                .FirstOrDefaultAsync(j => j.Id == dto.JobPostId && j.EmployerProfileId == employer.Id);

            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            var contract = new Contract
            {
                JobPostId = dto.JobPostId,
                EmployerProfileId = employer.Id,
                JobSeekerProfileId = dto.JobSeekerProfileId,
                Amount = dto.Amount,
                PaymentType = job.PaymentType ?? PaymentType.Fixed,
                Status = ContractStatus.Active
            };

            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var jobSeeker = await context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.Id == dto.JobSeekerProfileId);

            if (jobSeeker != null)
            {
                var notification = new Notification
                {
                    UserId = jobSeeker.UserId,
                    Title = "New Contract",
                    Message = $"You have been offered a contract for '{job.Title}'.",
                    Type = "contract"
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();

                await notificationHub.Clients
                    .Group($"user_{jobSeeker.UserId}")
                    .SendAsync("ReceiveNotification", new ReturnNotificationDto
                    {
                        Id = notification.Id,
                        Title = notification.Title,
                        Message = notification.Message,
                        IsRead = false,
                        Type = notification.Type,
                        CreatedAt = notification.CreatedAt
                    });
            }

            return Ok(ResponseModel<ReturnContractDto>.Ok(new ReturnContractDto
            {
                Id = contract.Id,
                JobPostId = contract.JobPostId,
                JobTitle = job.Title,
                JobSeekerProfileId = contract.JobSeekerProfileId,
                JobSeekerName = jobSeeker?.FullName,
                Amount = contract.Amount,
                Status = contract.Status,
                PaymentType = contract.PaymentType,
                CreatedAt = contract.CreatedAt
            }, "Contract created."));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyContracts()
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            List<ReturnContractDto> contracts;

            if (role == "Employer")
            {
                var employer = await context.EmployerProfiles
                    .FirstOrDefaultAsync(e => e.UserId == userId);

                if (employer == null)
                    return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

                contracts = await context.Contracts
                    .Include(c => c.JobPost)
                    .Include(c => c.JobSeekerProfile)
                    .Where(c => c.EmployerProfileId == employer.Id)
                    .Select(c => new ReturnContractDto
                    {
                        Id = c.Id,
                        JobPostId = c.JobPostId,
                        JobTitle = c.JobPost.Title,
                        JobSeekerProfileId = c.JobSeekerProfileId,
                        JobSeekerName = c.JobSeekerProfile.FullName,
                        Amount = c.Amount,
                        Status = c.Status,
                        PaymentType = c.PaymentType,
                        CompletedAt = c.CompletedAt,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }
            else
            {
                var jobSeeker = await context.JobSeekerProfiles
                    .FirstOrDefaultAsync(j => j.UserId == userId);

                if (jobSeeker == null)
                    return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

                contracts = await context.Contracts
                    .Include(c => c.JobPost)
                    .Include(c => c.EmployerProfile)
                    .Where(c => c.JobSeekerProfileId == jobSeeker.Id)
                    .Select(c => new ReturnContractDto
                    {
                        Id = c.Id,
                        JobPostId = c.JobPostId,
                        JobTitle = c.JobPost.Title,
                        JobSeekerProfileId = c.JobSeekerProfileId,
                        Amount = c.Amount,
                        Status = c.Status,
                        PaymentType = c.PaymentType,
                        CompletedAt = c.CompletedAt,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }

            return Ok(ResponseModel<List<ReturnContractDto>>.Ok(contracts));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await context.Contracts
                .Include(c => c.JobPost)
                .Include(c => c.JobSeekerProfile)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound(ResponseModel<string>.Fail("Contract not found."));

            return Ok(ResponseModel<ReturnContractDto>.Ok(new ReturnContractDto
            {
                Id = contract.Id,
                JobPostId = contract.JobPostId,
                JobTitle = contract.JobPost.Title,
                JobSeekerProfileId = contract.JobSeekerProfileId,
                JobSeekerName = contract.JobSeekerProfile.FullName,
                Amount = contract.Amount,
                Status = contract.Status,
                PaymentType = contract.PaymentType,
                CompletedAt = contract.CompletedAt,
                CreatedAt = contract.CreatedAt
            }));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateContract(int id, UpdateContractDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = GetUserId();
            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            var contract = await context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && c.EmployerProfileId == employer!.Id);

            if (contract == null)
                return NotFound(ResponseModel<string>.Fail("Contract not found."));

            contract.Status = dto.Status;

            if (dto.Status == ContractStatus.Completed)
                contract.CompletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Contract updated."));
        }
    }
}