using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public JobApplicationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<JobApplicationResult> ApplyAsync(int userId, int jobId, CreateJobApplicationDto dto)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null) return JobApplicationResult.ProfileNotFound;

            var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);
            if (job == null) return JobApplicationResult.JobNotFound;

            if (await _context.JobApplications.AnyAsync(a => a.JobPostId == jobId && a.JobSeekerProfileId == profile.Id))
                return JobApplicationResult.AlreadyApplied;

            _context.JobApplications.Add(new JobApplication
            {
                JobPostId = jobId,
                JobSeekerProfileId = profile.Id,
                CoverLetter = dto.CoverLetter,
                Status = ApplicationStatus.Applied
            });

            await _context.SaveChangesAsync();
            return JobApplicationResult.Success;
        }

        public async Task<List<ReturnJobApplicationDto>?> GetMyApplicationsAsync(int userId)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null) return null;

            return await _context.JobApplications
                .Where(a => a.JobSeekerProfileId == profile.Id)
                .ProjectTo<ReturnJobApplicationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}