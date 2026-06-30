using OnlineJobRecruitmentSystem.Application.DTOs.AnalyticsDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IAnalyticsService
    {
        Task<EmployerStatsDto> GetEmployerStatsAsync(int employerId);
    }
}