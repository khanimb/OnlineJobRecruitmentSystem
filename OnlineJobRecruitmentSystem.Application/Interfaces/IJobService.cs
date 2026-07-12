using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{

    public class PagedJobResult
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<ReturnJobDto> Data { get; set; } = new();
    }

    public interface IJobService
    {
        Task<PagedJobResult> GetJobsAsync(string? category, string? location, string? jobType, decimal? salaryMin, decimal? salaryMax, int page, int pageSize);
        Task<PlatformStatsDto> GetPublicStatsAsync();
        Task<ReturnJobDto?> GetJobByIdAsync(int id);
        Task<JobOperationResult> CreateJobAsync(int userId, CreateJobDto dto);
        Task<JobOperationResult> UpdateJobAsync(int id, int userId, UpdateJobDto dto);
        Task<JobOperationResult> DeleteJobAsync(int id, int userId);
        Task<List<ReturnJobDto>?> GetSimilarJobsAsync(int id);
        Task<List<ReturnJobDto>> GetMyJobsAsync(int userId);
    }
}