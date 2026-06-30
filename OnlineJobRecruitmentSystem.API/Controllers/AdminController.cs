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

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
                return NotFound(ResponseModel<string>.Fail("User not found."));

            return Ok(ResponseModel<object>.Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                user.Role
            }));
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

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await context.JobPosts
                .Include(j => j.EmployerProfile)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Location,
                    j.IsActive,
                    j.Deadline,
                    Company = j.EmployerProfile!.CompanyName
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(jobs));
        }

        [HttpDelete("jobs/{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await context.JobPosts.FindAsync(id);
            if (job == null)
                return NotFound(ResponseModel<string>.Fail("Job not found."));

            context.JobPosts.Remove(job);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Job deleted."));
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

        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews()
        {
            var reviews = await context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Select(r => new
                {
                    r.Id,
                    ReviewerEmail = r.Reviewer.Email,
                    RevieweeEmail = r.Reviewee.Email,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(reviews));
        }

        [HttpDelete("reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound(ResponseModel<string>.Fail("Review not found."));

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Review deleted."));
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await context.Payments
                .Include(p => p.Employer)
                .Select(p => new
                {
                    p.Id,
                    EmployerEmail = p.Employer.Email,
                    p.Plan,
                    p.Amount,
                    p.Status,
                    p.CreatedAt
                }).ToListAsync();

            return Ok(ResponseModel<object>.Ok(payments));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await context.Users.CountAsync();
            var totalJobs = await context.JobPosts.CountAsync();
            var activeJobs = await context.JobPosts.CountAsync(j => j.IsActive);
            var totalApplications = await context.JobApplications.CountAsync();
            var totalPayments = await context.Payments.CountAsync();
            var totalRevenue = await context.Payments
                .Where(p => p.Status == "completed")
                .SumAsync(p => p.Amount);

            return Ok(ResponseModel<object>.Ok(new
            {
                totalUsers,
                totalJobs,
                activeJobs,
                totalApplications,
                totalPayments,
                totalRevenue
            }));
        }
    }
}