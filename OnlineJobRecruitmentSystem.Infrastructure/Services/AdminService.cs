using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.AdminDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdminUserDto>> GetUsersAsync()
        {
            return await _context.Users
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Username = u.Username,
                    Role = u.Role,
                    IsEmailVerified = u.IsEmailVerified
                }).ToListAsync();
        }

        public async Task<AdminUserDto?> GetUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminJobDto>> GetJobsAsync()
        {
            return await _context.JobPosts
                .Include(j => j.EmployerProfile)
                .Select(j => new AdminJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location,
                    IsActive = j.IsActive,
                    Deadline = j.Deadline,
                    Company = j.EmployerProfile!.CompanyName
                }).ToListAsync();
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = await _context.JobPosts.FindAsync(id);
            if (job == null) return false;

            _context.JobPosts.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminApplicationDto>> GetApplicationsAsync()
        {
            return await _context.JobApplications
                .Include(a => a.JobPost)
                .Include(a => a.JobSeekerProfile)
                    .ThenInclude(j => j!.User)
                .Select(a => new AdminApplicationDto
                {
                    Id = a.Id,
                    JobTitle = a.JobPost!.Title,
                    ApplicantName = a.JobSeekerProfile!.User!.Username,
                    Status = a.Status.ToString()
                }).ToListAsync();
        }

        public async Task<List<AdminReviewDto>> GetReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Select(r => new AdminReviewDto
                {
                    Id = r.Id,
                    ReviewerEmail = r.Reviewer.Email,
                    RevieweeEmail = r.Reviewee.Email,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminContactMessageDto>> GetContactMessagesAsync()
        {
            return await _context.ContactMessages
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new AdminContactMessageDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Message = c.Message,
                    CreatedAt = c.CreatedAt,
                    IsRead = c.IsRead
                }).ToListAsync();
        }

        public async Task<bool> MarkContactMessageAsReadAsync(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return false;

            msg.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteContactMessageAsync(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return false;

            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminPaymentDto>> GetPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Employer)
                .Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    EmployerEmail = p.Employer.Email,
                    Plan = p.Plan,
                    Amount = p.Amount,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
        }

        public async Task<AdminStatsDto> GetStatsAsync()
        {
            return new AdminStatsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalJobs = await _context.JobPosts.CountAsync(),
                ActiveJobs = await _context.JobPosts.CountAsync(j => j.IsActive),
                TotalApplications = await _context.JobApplications.CountAsync(),
                TotalPayments = await _context.Payments.CountAsync(),
                TotalRevenue = await _context.Payments
                    .Where(p => p.Status == "completed")
                    .SumAsync(p => p.Amount)
            };
        }

        public async Task<List<AdminContractDto>> GetContractsAsync()
        {
            return await _context.Contracts
                .Include(c => c.JobPost)
                .Include(c => c.EmployerProfile)
                .Include(c => c.JobSeekerProfile)
                .Select(c => new AdminContractDto
                {
                    Id = c.Id,
                    JobTitle = c.JobPost.Title,
                    EmployerName = c.EmployerProfile.CompanyName,
                    JobSeekerName = c.JobSeekerProfile.FullName,
                    Amount = c.Amount,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                }).ToListAsync();
        }

        public async Task<AdminRoleUpdateResult> UpdateUserRoleAsync(int id, string role)
        {
            var validRoles = new[] { Roles.Admin, Roles.Employer, Roles.JobSeeker };
            if (!validRoles.Contains(role))
                return AdminRoleUpdateResult.InvalidRole;

            var user = await _context.Users.FindAsync(id);
            if (user == null) return AdminRoleUpdateResult.UserNotFound;

            user.Role = role;
            await _context.SaveChangesAsync();
            return AdminRoleUpdateResult.Success;
        }

        public async Task<bool> UpdateJobStatusAsync(int id, bool isActive)
        {
            var job = await _context.JobPosts.FindAsync(id);
            if (job == null) return false;

            job.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}