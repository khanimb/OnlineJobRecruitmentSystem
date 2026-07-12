using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public JobService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlatformStatsDto> GetPublicStatsAsync()
        {
            var totalJobs = await _context.JobPosts.CountAsync(j => j.IsActive);
            var totalCompanies = await _context.EmployerProfiles.CountAsync();
            var totalJobSeekers = await _context.JobSeekerProfiles.CountAsync();
            var totalApplications = await _context.JobApplications.CountAsync();
            var shortlisted = await _context.JobApplications.CountAsync(a => a.Status == ApplicationStatus.Shortlisted);
            var successRate = totalApplications > 0 ? Math.Round((double)shortlisted / totalApplications * 100, 0) : 0;

            return new PlatformStatsDto
            {
                TotalActiveJobs = totalJobs,
                TotalCompanies = totalCompanies,
                TotalJobSeekers = totalJobSeekers,
                SuccessRate = successRate
            };
        }

        public async Task<PagedJobResult> GetJobsAsync(string? category, string? location, string? jobType, decimal? salaryMin, decimal? salaryMax, int page, int pageSize)
        {
            var query = _context.JobPosts.Where(j => j.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(j => j.Category == category);
            if (!string.IsNullOrEmpty(location))
                query = query.Where(j => j.Location == location);
            if (!string.IsNullOrEmpty(jobType))
                query = query.Where(j => j.JobType == jobType);
            if (salaryMin.HasValue)
                query = query.Where(j => j.SalaryMin >= salaryMin);
            if (salaryMax.HasValue)
                query = query.Where(j => j.SalaryMax <= salaryMax);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<ReturnJobDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedJobResult
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Data = jobs
            };
        }

        public async Task<ReturnJobDto?> GetJobByIdAsync(int id)
        {
            return await _context.JobPosts
                .Where(j => j.Id == id)
                .ProjectTo<ReturnJobDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ReturnJobDto>> GetMyJobsAsync(int userId)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return new List<ReturnJobDto>();

            return await _context.JobPosts
                .Where(j => j.EmployerProfileId == employer.Id)
                .OrderByDescending(j => j.Id)
                .ProjectTo<ReturnJobDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<JobOperationResult> CreateJobAsync(int userId, CreateJobDto dto)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return JobOperationResult.EmployerNotFound;

            var job = new JobPost
            {
                EmployerProfileId = employer.Id,
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Location = dto.Location,
                JobType = dto.JobType,
                Category = dto.Category,
                SalaryMin = dto.SalaryMin,
                SalaryMax = dto.SalaryMax,
                Budget = dto.Budget,
                PaymentType = Enum.TryParse<PaymentType>(dto.PaymentType, out var pt) ? pt : null,
                Deadline = dto.Deadline
            };

            _context.JobPosts.Add(job);
            await _context.SaveChangesAsync();
            return JobOperationResult.Success;
        }

        public async Task<JobOperationResult> UpdateJobAsync(int id, int userId, UpdateJobDto dto)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return JobOperationResult.EmployerNotFound;

            var job = await _context.JobPosts
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == employer.Id);
            if (job == null) return JobOperationResult.JobNotFound;

            job.Title = dto.Title;
            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.Location = dto.Location;
            job.JobType = dto.JobType;
            job.Category = dto.Category;
            job.SalaryMin = dto.SalaryMin;
            job.SalaryMax = dto.SalaryMax;
            job.Deadline = dto.Deadline;
            job.Budget = dto.Budget;
            job.PaymentType = Enum.TryParse<PaymentType>(dto.PaymentType, out var pt) ? pt : null;
            job.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return JobOperationResult.Success;
        }

        public async Task<JobOperationResult> DeleteJobAsync(int id, int userId)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return JobOperationResult.EmployerNotFound;

            var job = await _context.JobPosts
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == employer.Id);
            if (job == null) return JobOperationResult.JobNotFound;

            _context.JobPosts.Remove(job);
            await _context.SaveChangesAsync();
            return JobOperationResult.Success;
        }

        public async Task<List<ReturnJobDto>?> GetSimilarJobsAsync(int id)
        {
            var job = await _context.JobPosts.FindAsync(id);
            if (job == null) return null;

            return await _context.JobPosts
                .Where(j => j.IsActive && j.Id != id &&
                            (j.Category == job.Category || j.JobType == job.JobType))
                .Take(5)
                .ProjectTo<ReturnJobDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}