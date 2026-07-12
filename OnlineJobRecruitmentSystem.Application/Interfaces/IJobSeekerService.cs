using Microsoft.AspNetCore.Http;
using OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{

    public interface IJobSeekerService
    {
        Task<ReturnJobSeekerDto?> CreateProfileAsync(int userId, CreateJobSeekerDto dto);
        Task<ReturnJobSeekerDto?> GetProfileAsync(int userId);
        Task<ReturnJobSeekerDto?> UpdateProfileAsync(int userId, UpdateJobSeekerDto dto);
        Task<string?> UploadCvAsync(int userId, IFormFile file);
        Task<List<ReturnSavedJobDto>?> GetSavedJobsAsync(int userId);
        Task<SaveJobResult> SaveJobAsync(int userId, int jobId);
        Task<RemoveSavedJobResult> RemoveSavedJobAsync(int userId, int jobId);
    }
}