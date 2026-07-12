using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class JobSeekerService : IJobSeekerService
    {
        private readonly AppDbContext _context;
        private readonly FileManager _fileManager;
        private readonly IMapper _mapper;

        public JobSeekerService(AppDbContext context, FileManager fileManager, IMapper mapper)
        {
            _context = context;
            _fileManager = fileManager;
            _mapper = mapper;
        }

        public async Task<ReturnJobSeekerDto?> CreateProfileAsync(int userId, CreateJobSeekerDto dto)
        {
            if (await _context.JobSeekerProfiles.AnyAsync(j => j.UserId == userId))
                return null;

            var profile = new JobSeekerProfile
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Skills = dto.Skills,
                WorkExperience = dto.WorkExperience
            };

            _context.JobSeekerProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return _mapper.Map<ReturnJobSeekerDto>(profile);
        }

        public async Task<ReturnJobSeekerDto?> GetProfileAsync(int userId)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            return profile == null ? null : _mapper.Map<ReturnJobSeekerDto>(profile);
        }

        public async Task<ReturnJobSeekerDto?> UpdateProfileAsync(int userId, UpdateJobSeekerDto dto)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null) return null;

            profile.FullName = dto.FullName;
            profile.Phone = dto.Phone;
            profile.Skills = dto.Skills;
            profile.WorkExperience = dto.WorkExperience;

            await _context.SaveChangesAsync();
            return _mapper.Map<ReturnJobSeekerDto>(profile);
        }

        public async Task<string?> UploadCvAsync(int userId, IFormFile file)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null) return null;

            if (!string.IsNullOrEmpty(profile.CvUrl))
                _fileManager.Delete(profile.CvUrl);

            profile.CvUrl = await _fileManager.UploadAsync(file, "cvs");
            await _context.SaveChangesAsync();

            return profile.CvUrl;
        }

        public async Task<List<ReturnSavedJobDto>?> GetSavedJobsAsync(int userId)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null) return null;

            return await _context.SavedJobs
                .Include(s => s.JobPost)
                    .ThenInclude(j => j!.EmployerProfile)
                .Where(s => s.JobSeekerProfileId == profile.Id)
                .Select(s => new ReturnSavedJobDto
                {
                    Id = s.Id,
                    SavedAt = s.SavedAt,
                    Job = new SavedJobInfoDto
                    {
                        Id = s.JobPost!.Id,
                        Title = s.JobPost.Title,
                        Location = s.JobPost.Location,
                        JobType = s.JobPost.JobType,
                        SalaryMin = s.JobPost.SalaryMin,
                        SalaryMax = s.JobPost.SalaryMax,
                        Deadline = s.JobPost.Deadline,
                        CompanyName = s.JobPost.EmployerProfile!.CompanyName
                    }
                }).ToListAsync();
        }

        public async Task<SaveJobResult> SaveJobAsync(int userId, int jobId)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null) return SaveJobResult.ProfileNotFound;

            var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);
            if (job == null) return SaveJobResult.JobNotFound;

            if (await _context.SavedJobs.AnyAsync(s => s.JobSeekerProfileId == profile.Id && s.JobPostId == jobId))
                return SaveJobResult.AlreadySaved;

            _context.SavedJobs.Add(new SavedJob
            {
                JobSeekerProfileId = profile.Id,
                JobPostId = jobId
            });

            await _context.SaveChangesAsync();
            return SaveJobResult.Success;
        }

        public async Task<RemoveSavedJobResult> RemoveSavedJobAsync(int userId, int jobId)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null) return RemoveSavedJobResult.ProfileNotFound;

            var saved = await _context.SavedJobs
                .FirstOrDefaultAsync(s => s.JobSeekerProfileId == profile.Id && s.JobPostId == jobId);

            if (saved == null) return RemoveSavedJobResult.SavedJobNotFound;

            _context.SavedJobs.Remove(saved);
            await _context.SaveChangesAsync();
            return RemoveSavedJobResult.Success;
        }
    }
}