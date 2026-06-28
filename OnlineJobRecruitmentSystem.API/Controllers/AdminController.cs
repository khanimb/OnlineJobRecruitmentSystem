using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController(AppDbContext context) : ControllerBase
    {
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.Username,
                    u.Role
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(users));
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
                return NotFound(ResponseModel<string>.Fail("User not found."));

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "User deleted."));
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications()
        {
            var apps = await context.JobApplications
                .Include(a => a.JobPost)
                .Include(a => a.JobSeekerProfile)
                    .ThenInclude(j => j!.User)
                .Select(a => new
                {
                    a.Id,
                    JobTitle = a.JobPost!.Title,
                    ApplicantName = a.JobSeekerProfile!.User!.Username,
                    Status = a.Status.ToString()
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(apps));
        }
    }
}