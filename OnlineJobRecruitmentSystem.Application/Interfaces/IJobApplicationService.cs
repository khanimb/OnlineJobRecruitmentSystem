using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{

    public interface IJobApplicationService
    {
        Task<JobApplicationResult> ApplyAsync(int userId, int jobId, CreateJobApplicationDto dto);
        Task<List<ReturnJobApplicationDto>?> GetMyApplicationsAsync(int userId);
    }
}