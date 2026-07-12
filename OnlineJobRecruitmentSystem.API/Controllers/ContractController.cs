using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineJobRecruitmentSystem.API.Hubs;
using OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractController(
        IContractService contractService,
        IHubContext<NotificationHub> notificationHub,
        INotificationService notificationService) : BaseApiController
    {
        [HttpPost]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> CreateContract(CreateContractDto dto)
        {
            var result = await contractService.CreateContractAsync(CurrentUserId, dto);

            if (result.Status == ContractResultStatus.EmployerNotFound)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));
            if (result.Status == ContractResultStatus.JobNotFound)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            var contract = result.Contract!;

            if (contract.JobSeekerUserId != 0)
            {
                var message = $"You have been offered a contract for '{contract.JobTitle}'.";

                await notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = contract.JobSeekerUserId,
                    Title = "New Contract",
                    Message = message,
                    Type = "contract"
                });

                await notificationHub.Clients
                    .Group($"user_{contract.JobSeekerUserId}")
                    .SendAsync("ReceiveNotification", new ReturnNotificationDto
                    {
                        Title = "New Contract",
                        Message = message,
                        IsRead = false,
                        Type = "contract",
                        CreatedAt = DateTime.UtcNow
                    });
            }

            return Ok(ResponseModel<ReturnContractDto>.Ok(contract, "Contract created."));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyContracts()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await contractService.GetMyContractsAsync(CurrentUserId, role);

            if (result.ProfileNotFound)
                return NotFound(ResponseModel<string>.Fail(role == "Employer" ? "Employer profile not found." : "Job seeker profile not found."));

            return Ok(ResponseModel<List<ReturnContractDto>>.Ok(result.Contracts));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var result = await contractService.GetContractAsync(id, CurrentUserId);

            return result.Status switch
            {
                ContractAccessStatus.NotFound => NotFound(ResponseModel<string>.Fail("Contract not found.")),
                ContractAccessStatus.Forbidden => Forbid(),
                _ => Ok(ResponseModel<ReturnContractDto>.Ok(result.Contract!))
            };
        }

        [HttpPut("{id}")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> UpdateContract(int id, UpdateContractDto dto)
        {
            var status = await contractService.UpdateContractStatusAsync(id, CurrentUserId, dto);

            return status switch
            {
                ContractUpdateStatus.EmployerNotFound => NotFound(ResponseModel<string>.Fail("Employer profile not found.")),
                ContractUpdateStatus.ContractNotFound => NotFound(ResponseModel<string>.Fail("Contract not found.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Contract updated."))
            };
        }
    }
}