using OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IJobAlertService
    {
        Task<ReturnJobAlertDto> CreateAlertAsync(int userId, CreateJobAlertDto dto);
        Task<List<ReturnJobAlertDto>> GetUserAlertsAsync(int userId);
        Task<ReturnJobAlertDto?> GetAlertByIdAsync(int alertId, int userId);
        Task<bool> UpdateAlertAsync(int alertId, int userId, UpdateJobAlertDto dto);
        Task<bool> DeleteAlertAsync(int alertId, int userId);
        Task SendJobAlertsAsync(string frequency);
    }
}