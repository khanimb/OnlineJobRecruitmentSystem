using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.EmployerDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class EmployerService : IEmployerService
    {
        private readonly AppDbContext _context;
        private readonly FileManager _fileManager;
        private readonly IMapper _mapper;

        private static readonly Dictionary<ApplicationStatus, ApplicationStatus[]> AllowedStatusTransitions = new()
        {
            [ApplicationStatus.Applied] = new[] { ApplicationStatus.Reviewed },
            [ApplicationStatus.Reviewed] = new[] { ApplicationStatus.Shortlisted, ApplicationStatus.Rejected },
            [ApplicationStatus.Shortlisted] = Array.Empty<ApplicationStatus>(),
            [ApplicationStatus.Rejected] = Array.Empty<ApplicationStatus>()
        };

        public EmployerService(AppDbContext context, FileManager fileManager, IMapper mapper)
        {
            _context = context;
            _fileManager = fileManager;
            _mapper = mapper;
        }

        public async Task<ReturnEmployerDto?> CreateProfileAsync(int userId, CreateEmployerDto dto)
        {
            if (await _context.EmployerProfiles.AnyAsync(e => e.UserId == userId))
                return null;

            var profile = new EmployerProfile
            {
                UserId = userId,
                CompanyName = dto.CompanyName,
                Description = dto.Description,
                Website = dto.Website
            };

            _context.EmployerProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return _mapper.Map<ReturnEmployerDto>(profile);
        }

        public async Task<ReturnEmployerDto?> GetProfileAsync(int userId)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            return profile == null ? null : _mapper.Map<ReturnEmployerDto>(profile);
        }

        public async Task<ReturnEmployerDto?> UpdateProfileAsync(int userId, UpdateEmployerDto dto)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (profile == null) return null;

            profile.CompanyName = dto.CompanyName;
            profile.Description = dto.Description;
            profile.Website = dto.Website;

            await _context.SaveChangesAsync();
            return _mapper.Map<ReturnEmployerDto>(profile);
        }

        public async Task<string?> UploadLogoAsync(int userId, IFormFile file)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (profile == null) return null;

            if (!string.IsNullOrEmpty(profile.LogoUrl))
                _fileManager.Delete(profile.LogoUrl);

            profile.LogoUrl = await _fileManager.UploadAsync(file, "logos");
            await _context.SaveChangesAsync();

            return profile.LogoUrl;
        }

        public async Task<List<ReturnEmployerApplicationDto>?> GetApplicationsAsync(int userId, string? status)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return null;

            var query = _context.JobApplications
                .Include(a => a.JobPost)
                .Include(a => a.JobSeekerProfile)
                .Where(a => a.JobPost!.EmployerProfileId == employer.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var statusEnum))
                query = query.Where(a => a.Status == statusEnum);

            return await query.Select(a => new ReturnEmployerApplicationDto
            {
                Id = a.Id,
                Status = a.Status,
                CoverLetter = a.CoverLetter,
                Notes = a.Notes,
                AppliedAt = a.AppliedAt,
                JobPostId = a.JobPostId,
                JobTitle = a.JobPost!.Title,
                JobSeeker = new ReturnEmployerApplicantDto
                {
                    Id = a.JobSeekerProfile!.Id,
                    UserId = a.JobSeekerProfile.UserId,
                    FullName = a.JobSeekerProfile.FullName,
                    Skills = a.JobSeekerProfile.Skills,
                    CvUrl = a.JobSeekerProfile.CvUrl
                }
            }).ToListAsync();
        }

        public async Task<EmployerJobApplicantsResult> GetApplicationsByJobAsync(int userId, int jobId)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return new EmployerJobApplicantsResult { EmployerNotFound = true };

            var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == employer.Id);
            if (job == null) return new EmployerJobApplicantsResult { JobNotFound = true };

            var list = await _context.JobApplications
                .Include(a => a.JobSeekerProfile)
                .Where(a => a.JobPostId == jobId)
                .Select(a => new ReturnJobApplicantDto
                {
                    Id = a.Id,
                    Status = a.Status,
                    CoverLetter = a.CoverLetter,
                    Notes = a.Notes,
                    AppliedAt = a.AppliedAt,
                    JobSeeker = new JobApplicantSummaryDto
                    {
                        FullName = a.JobSeekerProfile!.FullName,
                        Skills = a.JobSeekerProfile.Skills,
                        CvUrl = a.JobSeekerProfile.CvUrl
                    }
                }).ToListAsync();

            return new EmployerJobApplicantsResult { Applicants = list };
        }

        public async Task<EmployerApplicationStatusUpdateResult> UpdateApplicationStatusAsync(int userId, int applicationId, ApplicationStatus status, string notes)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return new EmployerApplicationStatusUpdateResult { EmployerNotFound = true };

            var application = await _context.JobApplications
                .Include(a => a.JobPost)
                .Include(a => a.JobSeekerProfile)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost!.EmployerProfileId == employer.Id);

            if (application == null) return new EmployerApplicationStatusUpdateResult { ApplicationNotFound = true };

            if (!AllowedStatusTransitions.TryGetValue(application.Status, out var allowed) || !allowed.Contains(status))
                return new EmployerApplicationStatusUpdateResult { InvalidTransition = true, CurrentStatus = application.Status.ToString() };

            application.Status = status;
            application.Notes = notes;
            await _context.SaveChangesAsync();

            var jobSeeker = await _context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.Id == application.JobSeekerProfileId);

            return new EmployerApplicationStatusUpdateResult
            {
                JobSeekerEmail = jobSeeker?.User?.Email,
                JobSeekerUserId = jobSeeker?.UserId ?? 0,
                JobTitle = application.JobPost!.Title,
                NewStatus = application.Status.ToString()
            };
        }

        public async Task<EmployerDashboardDto?> GetDashboardAsync(int userId)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return null;

            var totalJobs = await _context.JobPosts.CountAsync(j => j.EmployerProfileId == employer.Id);
            var totalApplications = await _context.JobApplications
                .Include(a => a.JobPost)
                .CountAsync(a => a.JobPost!.EmployerProfileId == employer.Id);
            var shortlisted = await _context.JobApplications
                .Include(a => a.JobPost)
                .CountAsync(a => a.JobPost!.EmployerProfileId == employer.Id && a.Status == ApplicationStatus.Shortlisted);
            var rejected = await _context.JobApplications
                .Include(a => a.JobPost)
                .CountAsync(a => a.JobPost!.EmployerProfileId == employer.Id && a.Status == ApplicationStatus.Rejected);

            return new EmployerDashboardDto
            {
                TotalJobs = totalJobs,
                TotalApplications = totalApplications,
                Shortlisted = shortlisted,
                Rejected = rejected
            };
        }
    }
}