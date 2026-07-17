using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Admin)]
    public class AdminController(IAdminService adminService) : BaseApiController
    {
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
            => Ok(ResponseModel<object>.Ok(await adminService.GetUsersAsync()));

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await adminService.GetUserAsync(id);
            if (user == null)
                return NotFound(ResponseModel<string>.Fail("User not found."));

            return Ok(ResponseModel<object>.Ok(user));
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await adminService.DeleteUserAsync(id);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("User not found."));

            return Ok(ResponseModel<string>.Ok(null!, "User deleted."));
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs()
            => Ok(ResponseModel<object>.Ok(await adminService.GetJobsAsync()));

        [HttpDelete("jobs/{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var deleted = await adminService.DeleteJobAsync(id);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Job deleted."));
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications()
            => Ok(ResponseModel<object>.Ok(await adminService.GetApplicationsAsync()));

        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews()
            => Ok(ResponseModel<object>.Ok(await adminService.GetReviewsAsync()));

        [HttpDelete("reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var deleted = await adminService.DeleteReviewAsync(id);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("Review not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Review deleted."));
        }

        [HttpGet("contact-messages")]
        public async Task<IActionResult> GetContactMessages()
            => Ok(ResponseModel<object>.Ok(await adminService.GetContactMessagesAsync()));

        [HttpDelete("contact-messages/{id}")]
        public async Task<IActionResult> DeleteContactMessage(int id)
        {
            var deleted = await adminService.DeleteContactMessageAsync(id);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("Message not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Message deleted."));
        }

        [HttpPut("contact-messages/{id}/read")]
        public async Task<IActionResult> MarkMessageRead(int id)
        {
            var updated = await adminService.MarkContactMessageAsReadAsync(id);
            if (!updated)
                return NotFound(ResponseModel<string>.Fail("Message not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Marked as read."));
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
            => Ok(ResponseModel<object>.Ok(await adminService.GetPaymentsAsync()));

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
            => Ok(ResponseModel<object>.Ok(await adminService.GetStatsAsync()));

        [HttpGet("contracts")]
        public async Task<IActionResult> GetContracts()
            => Ok(ResponseModel<object>.Ok(await adminService.GetContractsAsync()));

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] string role)
        {
            var result = await adminService.UpdateUserRoleAsync(id, role);
            return result switch
            {
                AdminRoleUpdateResult.InvalidRole => BadRequest(ResponseModel<string>.Fail("Invalid role.")),
                AdminRoleUpdateResult.UserNotFound => NotFound(ResponseModel<string>.Fail("User not found.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "User role updated."))
            };
        }

        [HttpPut("jobs/{id}/status")]
        public async Task<IActionResult> UpdateJobStatus(int id, [FromBody] bool isActive)
        {
            var updated = await adminService.UpdateJobStatusAsync(id, isActive);
            if (!updated)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Job status updated."));
        }
    }
}