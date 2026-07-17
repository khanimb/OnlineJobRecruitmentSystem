using Microsoft.AspNetCore.Http;
using OnlineJobRecruitmentSystem.Application.DTOs.EmployerDtos;
using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public class EmployerJobApplicantsResult
    {
        public bool EmployerNotFound { get; set; }
        public bool JobNotFound { get; set; }
        public List<ReturnJobApplicantDto> Applicants { get; set; } = new();
    }

    public class EmployerApplicationStatusUpdateResult
    {
        public bool EmployerNotFound { get; set; }
        public bool ApplicationNotFound { get; set; }
        public bool InvalidTransition { get; set; }
        public string? CurrentStatus { get; set; }
        public string? JobSeekerEmail { get; set; }
        public int JobSeekerUserId { get; set; }
        public string? JobTitle { get; set; }
        public string? NewStatus { get; set; }
    }

    public interface IEmployerService
    {
        Task<ReturnEmployerDto?> CreateProfileAsync(int userId, CreateEmployerDto dto);
        Task<ReturnEmployerDto?> GetProfileAsync(int userId);
        Task<ReturnEmployerDto?> UpdateProfileAsync(int userId, UpdateEmployerDto dto);
        Task<string?> UploadLogoAsync(int userId, IFormFile file);
        Task<List<ReturnEmployerApplicationDto>?> GetApplicationsAsync(int userId, string? status);
        Task<EmployerJobApplicantsResult> GetApplicationsByJobAsync(int userId, int jobId);
        Task<EmployerApplicationStatusUpdateResult> UpdateApplicationStatusAsync(int userId, int applicationId, ApplicationStatus status, string notes);
        Task<EmployerDashboardDto?> GetDashboardAsync(int userId);
        Task<List<PublicCompanyDto>> GetPublicCompaniesAsync();
    }
}